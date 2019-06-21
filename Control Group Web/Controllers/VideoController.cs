using Control_Group_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApiAiSDK;
using ApiAiSDK.Model;

namespace Control_Group_Web.Controllers
{
    public class VideoController : Controller
    {
        public Dictionary<School, Dictionary<Committee, List<Video>>> schools = new Dictionary<School, Dictionary<Committee, List<Video>>>();
        public Dictionary<Committee, List<Video>> committees = new Dictionary<Committee, List<Video>>();
        private ApiAi apiAi;


        public ActionResult StartShow()
        {
            return View();
        }


        public ActionResult PrepareForVideo(RequestType id)
        {
            string[] data = id.order.Split(new char[] { ',' });
            VideoToShow VTS = new VideoToShow();
            VTS.admin = (string)Session["admin"];
            VTS.school = data[0];
            VTS.committee = data[1];
            VTS.url = data[2];
            Session["VideoToShow"] = VTS;
            return Json(new { success = true, responseText = "Your message successfuly sent!" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Play(RequestType request)
        {
            if (NoMoreVideos())
            {
                return Json(new { success = false, responseText = "Your message successfuly sent!" }, JsonRequestBehavior.AllowGet);
                //return RedirectToAction("StartShow");
            }
            else
            {
                switch (request.order)
                {
                    case "school":
                        ShuffleSchools();
                        break;
                    case "committee":
                        if(Session["shuffle_time"] != null)
                        {
                            if((int)Session["shuffle_time"] == 3)
                            {
                                Session["shuffle_time"] = 0;
                                RequestType rt = new RequestType();
                                rt.order = "school";
                                Play(rt);
                            }
                            else
                            {
                                Session["shuffle_time"] = ((int)Session["shuffle_time"]) + 1;
                                ShuffleCommittees();
                            }
                        }
                        else
                        {
                            Session["shuffle_time"] = 1;
                            ShuffleCommittees();
                        }
                        break;
                }
                return Json(new { success = true, responseText = "Your message successfuly sent!" }, JsonRequestBehavior.AllowGet);
            }
            
        }

        public bool NoMoreVideos()
        {
            int school_count = 0;
            int empty_school_count = 0;
            schools = (Dictionary<School, Dictionary<Committee, List<Video>>>)Session["videos"];
            foreach (KeyValuePair<School, Dictionary<Committee, List<Video>>> p1 in schools)
            {
                    school_count++;
                    int videosCount = 0;
                    foreach (KeyValuePair<Committee, List<Video>> p2 in p1.Value)
                    {
                            foreach (Video p3 in p2.Value)
                            {
                                if (!p3.seen)
                                {
                                    videosCount++;
                                }
                            }
                    }
                    if (videosCount <= 0)
                    {
                         empty_school_count++;
                    }
                }
            Session["videos"] = schools;
            if (empty_school_count == school_count)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public void ShuffleSchools()
        {
            schools = (Dictionary<School, Dictionary<Committee, List<Video>>>)Session["videos"];

            StopAllSchools();
            ActiveSomeSchools();
            ShuffleCommittees();
            checkForEmptyArea();
        }

        public void checkForEmptyArea()
        {
            int school_count = 0;
            int empty_school_count = 0;
            foreach (KeyValuePair<School, Dictionary<Committee, List<Video>>> p1 in schools)
            {
                if (p1.Key.active)
                {
                    school_count++;
                    int videosCount = 0;
                    foreach (KeyValuePair<Committee, List<Video>> p2 in p1.Value)
                    {
                        if (p2.Key.active)
                        {
                            foreach (Video p3 in p2.Value)
                            {
                                if (!p3.seen && p3.active)
                                {
                                    videosCount++;
                                }
                            }
                        }
                    }
                    if(videosCount <= 0)
                    {
                        if(!canAddVideos(p1.Key))
                        {
                            deactivate(p1.Key);
                            empty_school_count++;
                        }
                    }
                }
            }
            Session["videos"] = schools;
            if (empty_school_count == school_count)
            {
                RequestType rt = new RequestType();
                rt.order = "committee";
                Play(rt);
            }
           
        }

        public void deactivate(School key)
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

        public void ShuffleCommittees()
        {
            schools = (Dictionary<School, Dictionary<Committee, List<Video>>>)Session["videos"];
            StopAllCommittees();

            foreach (KeyValuePair<School, Dictionary<Committee, List<Video>>> p1 in schools)
            {
                if (p1.Key.active)
                {
                    int score = 0;
                    int lowest_views = p1.Value.Keys.Min(committee => committee.views);
                    int numToActive = (int)Session["committee_per_school"];
                    int stop = Math.Min(schools.Count(), numToActive);

                    foreach (KeyValuePair<Committee, List<Video>> p2 in p1.Value)
                    {
                        while (score < stop)
                        {
                            score += CustomActivecommitee(lowest_views, score, p1.Value, numToActive);
                            lowest_views++;
                        }
                    }
                }
            }
            ActiveSomeVideos();
            checkForEmptyArea();
            Session["videos"] = schools;

            
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
                    while (score < stop)
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



        private int GetAvailableVideosCount()
        {
            int videosCount = 0;

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


        private int CustomActivecommitee(int lowest_views, int current_score, Dictionary<Committee, List<Video>> committees, int numToActive)
        {
            int i = 0;
            foreach (KeyValuePair<Committee, List<Video>> p in committees)
            {
                if (i < (numToActive - current_score) && p.Key.views == lowest_views)
                {
                    p.Key.active = true;
                    i++;
                }
            }
            return i;
        }

        public void StopAllCommittees()
        {
            if (Session["videos"] != null)
            {
                schools = (Dictionary<School, Dictionary<Committee, List<Video>>>)Session["videos"];
                foreach (KeyValuePair<School, Dictionary<Committee, List<Video>>> p1 in schools)
                {
                    foreach (KeyValuePair<Committee, List<Video>> p2 in p1.Value)
                    {
                        p2.Key.active = false;
                    }
                }
            }
        }

        public void StopAllSchools()
        {
            foreach (KeyValuePair<School, Dictionary<Committee, List<Video>>> p in ((Dictionary<School, Dictionary<Committee, List<Video>>>) Session["videos"]))
            {
                p.Key.active = false;
            }
        }


        private void ActiveSomeSchools()
        {
            int score = 0;
            int lowest_views = schools.Keys.Min(school => school.views);
            int numToActive = (int)Session["school_per_page"];
            int stop = Math.Min(schools.Count(), numToActive);
            

                while (score < stop)
                {
                    score += CustomActiveSchool(lowest_views, score, schools,numToActive);
                    lowest_views++;
                }

            Session["videos"] = schools;
            
        }


        private int CustomActiveSchool(int lowest_views, int current_score, Dictionary<School, Dictionary<Committee, List<Video>>> schools, int numToActive)
        {
            int i = 0;
            foreach (KeyValuePair<School, Dictionary<Committee, List<Video>>> p in schools)
            {
                if (i < (numToActive - current_score) && p.Key.views == lowest_views)
                {
                    p.Key.active = true;
                    i++;
                }
            }
            return i;
        }



        public ActionResult show()
        {
            return View();
        }


        // GET: Video
        public ActionResult Index()
        {
            //RequestType rt = new RequestType();
            //rt.order = "committee";
            //Play(rt);
            return View();
        }

        // GET: Video/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Video/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Video/Create
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

        // GET: Video/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Video/Edit/5
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

        // GET: Video/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Video/Delete/5
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
        
        public ActionResult parse_and_response(speech_text data)
        {

            try
            {
                var config = new AIConfiguration("464272fea0564f81ac07d44d464efd2e", SupportedLanguage.English);
                Dictionary<string, string> parm = new Dictionary<string, string>();
                apiAi = new ApiAi(config);
                var response = apiAi.TextRequest(data.speech);
                foreach (var param in response.Result.Parameters)
                {
                    if (param.Value.ToString() == "")
                    {
                        parm.Add(param.Key.ToString(), "1");
                        continue;
                    }
                    parm.Add(param.Key.ToString(), param.Value.ToString());
                }
                return Json(new { success = true, responseText = "Your message successfuly sent!", committe_nubmer = parm["comm_number"], school_number = parm["school_number"], video_number = parm["video_number"] }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false, responseText = "some thing went wrong !" }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
