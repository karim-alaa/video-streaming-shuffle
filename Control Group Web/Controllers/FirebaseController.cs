using Control_Group_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Control_Group_Web.Controllers
{
    public class FirebaseController : Controller
    {
        public Dictionary<string, Dictionary<string, List<string>>> videos = new Dictionary<string, Dictionary<string, List<string>>>();
        public int video_per_school;
        public int school_per_page;
        public int committe_per_school;


        // GET: Firebase
        public ActionResult Index()
        {
            return View();
        }

        public void ResetVideosSession()
        {
            Dictionary<School, Dictionary<Committee, List<Video>>> schools = new Dictionary<School, Dictionary<Committee, List<Video>>>();
            schools = (Dictionary<School, Dictionary<Committee, List<Video>>>)Session["videos"];

            foreach (KeyValuePair<School, Dictionary<Committee, List<Video>>> p1 in schools)
            {
                p1.Key.active = false;
                p1.Key.views = 0;

                foreach (KeyValuePair<Committee, List<Video>> p2 in p1.Value)
                {
                    p2.Key.active = false;
                    p2.Key.views = 0;
                    foreach (Video p3 in p2.Value)
                    {
                        p3.active = false;
                        p3.seen = false;
                    }
                }
            }
        }

        public ActionResult ShowAgain()
        {

            school_per_page = (int)Session["school_per_page"];
            committe_per_school = (int)Session["committee_per_school"];

            ResetVideosSession();
            ActiveSomeSchools(school_per_page);
            ActiveSomeCommittees(committe_per_school);
            ActiveSomeVideos();
            checkForEmptyArea();
            return RedirectToAction("Index", "Video");
        }

        public ActionResult Login()
        {
            if (Session["user"] != null)
                return RedirectToAction("Index");
            else
                return View();
        }

        public ActionResult Logout()
        {
            Session["user"] = null;
            Session["admin"] = null;

            return RedirectToAction("login"); 
        }


        public ActionResult showRemaningVideos()
        {
            return View();
            //RequestType rt = new RequestType();
            //rt.order = "committee";
            //return RedirectToAction("Index", "Video", new { request = rt });
        }

        public ActionResult ShowAllVideos()
        {
            return RedirectToRoute("../Firebase/Index");
        }



        public ActionResult User_Data(User user)
        {

            Session["admin"] = "admin_"+user.Adamin_num;
            Session["user"] = user;
            RedirectToRoute("/firebase");
            return Json(new { success = true, responseText = "Your message successfuly sent!" }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CreateCover(FirebaseTest coordinates)
        {
            return null;
        }


        public ActionResult CreateCover2(object data)
        {
            if (data == null)
                return null;
            return null;
        }

        public ActionResult CreateList(DataTransfer data)
        {
            List<Video> urls = new List<Video>();
            Dictionary<Committee, List<Video>> committes = new Dictionary<Committee, List<Video>>();
            Dictionary<School, Dictionary<Committee, List<Video>>> last_videos_format = new Dictionary<School, Dictionary<Committee, List<Video>>>();
            string school_name = "school_";
            string committe_name = "committee_";
            int i = 1, j = 1;
            foreach (string url in data.videos_urls)
            {

                if (url == "##")
                {
                    committe_name += i;
                    Committee committee = new Committee();
                    committee.name = committe_name;
                    committes.Add(committee, urls);
                    committe_name = "committee_";
                    i++;
                    urls = new List<Video>();
                    continue;
                }
                if (url == "$$")
                {
                    school_name += j;
                    School school = new School();
                    school.name = school_name;
                    last_videos_format.Add(school, committes);
                    school_name = "school_";
                    j++;
                    i = 1;
                    committes = new Dictionary<Committee, List<Video>>();
                    urls = new List<Video>();
                    continue;
                }
                Video video = new Video();
                video.url = url;
                urls.Add(video);
            }

            video_per_school = 5;
            school_per_page = 2;
            committe_per_school = 4;
            if (Session["videos"] == null)
            {
                // build the first urls session
                Session["videos"] = last_videos_format;
                Session["temp"] = last_videos_format;
                Session["video_per_school"] = video_per_school;
                Session["school_per_page"] = school_per_page;
                Session["committee_per_school"] = committe_per_school;
                
            }
            else
            {
                //new data coming o.O
            }
            
            ActiveSomeSchools(school_per_page);
            ActiveSomeCommittees(committe_per_school);
            ActiveSomeVideos();
            checkForEmptyArea();
            return Json(new { success = true, responseText = "Your message successfuly sent!" }, JsonRequestBehavior.AllowGet);

        }


        public void checkForEmptyArea()
        {
            Dictionary<School, Dictionary<Committee, List<Video>>> schools = new Dictionary<School, Dictionary<Committee, List<Video>>>();
            schools = (Dictionary<School, Dictionary<Committee, List<Video>>>)Session["videos"];
            foreach (KeyValuePair<School, Dictionary<Committee, List<Video>>> p1 in schools)
            {
                if (p1.Key.active)
                {
                    int videosCount = 0;
                    foreach (KeyValuePair<Committee, List<Video>> p2 in p1.Value)
                    {
                        if (p2.Key.active)
                        {
                            foreach (Video p3 in p2.Value)
                            {
                                if (!p3.seen && !p3.active)
                                {
                                    videosCount++;
                                }
                            }
                        }
                    }
                    if (videosCount <= 0)
                    {
                        if (!canAddVideos(p1.Key))
                        {
                            deactivate(p1.Key,schools);
                        }
                    }
                }
            }
            Session["videos"] = schools;
        }


        public void deactivate(School key, Dictionary<School, Dictionary<Committee, List<Video>>> schools)
        {
            foreach (KeyValuePair<School, Dictionary<Committee, List<Video>>> p1 in schools)
            {
                if (p1.Key == key)
                {
                    p1.Key.active = false;
                    foreach (KeyValuePair<Committee, List<Video>> p2 in p1.Value)
                    {
                        if (p2.Key.active)
                        {
                            p2.Key.active = false;
                            foreach (Video p3 in p2.Value)
                            {
                                if (!p3.seen && p3.active)
                                {
                                    p3.active = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool canAddVideos(School key)
        {

            //foreach (KeyValuePair<School, Dictionary<Committee, List<Video>>> p1 in schools)
            //{
            //    if (p1.Key == key)
            //    {
            //        int videosCount = 0;
            //        foreach (KeyValuePair<Committee, List<Video>> p2 in p1.Value)
            //        {
            //            if (p2.Key.active)
            //            {
            //                foreach (Video p3 in p2.Value)
            //                {
            //                    if (!p3.seen && !p3.active)
            //                    {
            //                        videosCount++;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}


            return false;
        }



        private void ActiveSomeVideos()
        {
            int MaxNumOfVideos = (int)Session["video_per_school"];
            //int score = 0;
            //int stop = Math.Min(GetAvailableVideosCount(), MaxNumOfVideos);

            Dictionary<School, Dictionary<Committee, List<Video>>> schools = new Dictionary<School, Dictionary<Committee, List<Video>>>();
            schools = (Dictionary<School, Dictionary<Committee, List<Video>>>)Session["videos"];
            
                foreach (KeyValuePair<School, Dictionary<Committee, List<Video>>> p1 in schools)
                {
                    if (p1.Key.active)
                    {
                        int score = 0;
                    
                        int stop = Math.Min(GetAvailableVideosCountInSchool(p1.Key), MaxNumOfVideos);
                        while(score < stop)
                        {
                            foreach (KeyValuePair<Committee, List<Video>> p2 in p1.Value)
                            {
                                if (p2.Key.active)
                                {
                                    if (score < stop)
                                    {
                                        bool FoundAVideo = false;
                                        foreach (Video p3 in p2.Value)
                                        {
                                            if (!p3.seen && !p3.active && !FoundAVideo)
                                            {
                                                p3.active = true;
                                                score++;
                                                FoundAVideo = true;
                                            }
                                        }
                                    }
                                    else { continue; }
                                }
                            }
                        }
                    }
                }
            
            Session["videos"] = schools;
        }

        private int GetAvailableVideosCount()
        {
            int videosCount = 0;

            Dictionary<School, Dictionary<Committee, List<Video>>> schools = new Dictionary<School, Dictionary<Committee, List<Video>>>();
            schools = (Dictionary<School, Dictionary<Committee, List<Video>>>)Session["videos"];

            foreach (KeyValuePair<School, Dictionary<Committee, List<Video>>> p1 in schools)
            {
                if (p1.Key.active)
                {
                    foreach (KeyValuePair<Committee, List<Video>> p2 in p1.Value)
                    {
                        if (p2.Key.active)
                        {
                            foreach (Video p3 in p2.Value)
                            {
                                if (!p3.seen && !p3.active)
                                {
                                    videosCount++;
                                }
                            }
                        }
                    }
                }
            }
            return videosCount;
        }

        private int GetAvailableVideosCountInSchool(School key)
        {
            int videosCount = 0;

            Dictionary<School, Dictionary<Committee, List<Video>>> schools = new Dictionary<School, Dictionary<Committee, List<Video>>>();
            schools = (Dictionary<School, Dictionary<Committee, List<Video>>>)Session["videos"];

            foreach (KeyValuePair<School, Dictionary<Committee, List<Video>>> p1 in schools)
            {
                if (p1.Key == key)
                {
                    foreach (KeyValuePair<Committee, List<Video>> p2 in p1.Value)
                    {
                        if (p2.Key.active)
                        {
                            foreach (Video p3 in p2.Value)
                            {
                                if (!p3.seen && !p3.active)
                                {
                                    videosCount++;
                                }
                            }
                        }
                    }
                }
            }
            return videosCount;
        }

        public void ActiveSomeCommittees(int numToActive)
        {
            Dictionary<School, Dictionary<Committee, List<Video>>> temp = (Dictionary<School, Dictionary<Committee, List<Video>>>)Session["videos"];
            
            foreach (KeyValuePair<School, Dictionary<Committee, List<Video>>> p1 in temp)
            {
                int score = 0;
                if (p1.Key.active)
                {
                    foreach(KeyValuePair<Committee,List<Video>> p2 in p1.Value)
                    {
                        if (score < numToActive)
                        {
                            score++;
                            p2.Key.active = true;
                        }
                    }
                    
                }
                
            }
            Session["Videos"] = temp;
        }

        public void ActiveSomeSchools(int numToActive)
        {
            Dictionary<School, Dictionary<Committee, List<Video>>> temp = (Dictionary<School, Dictionary<Committee, List<Video>>>) Session["videos"];
            int score = 0;
            foreach(KeyValuePair<School,Dictionary<Committee,List<Video>>> p in temp) {
                if(score < numToActive)
                {
                    score++;
                    p.Key.active = true;
                }
            }
            Session["Videos"] = temp;
        }

      


        // GET: Firebase/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Firebase/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Firebase/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Firebase/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Firebase/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Firebase/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Firebase/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
