using QuizzApplication.viewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text;

namespace QuizzApplication.Controllers
{
    public class QuizzController : Controller
    {
        // GET: Quizz
        public DBQuizEntities dbContext = new DBQuizEntities();

        // GET: Quizz
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetUser(UserVM user)
        {
            UserVM userConnected = dbContext.Users.Where(u => u.FullName == user.FullName)
                                         .Select(u => new UserVM
                                         {
                                             UserID = u.UserID,
                                             FullName = u.FullName,
                                             ProfilImage = u.ProfilImage,

                                         }).FirstOrDefault();

            if (userConnected != null)
            {
                Session["UserConnected"] = userConnected;
                return RedirectToAction("SelectQuizz");
            }
            else
            {
                ViewBag.Msg = "Sorry : user is not found !!";
                return View();
            }

        }

        public ActionResult Register()
        {
            return View();
        }

        public ActionResult RegisterUser(
            string FullName,
            string ProfileImage
            )
        {
            string message = "";
            using (var db = new DBQuizEntities())
            {
                var users = db.Set<User>();
                users.Add(new User { FullName = FullName, ProfilImage = ProfileImage });
                int status = db.SaveChanges();
                if (status > 0)
                {
                    message = "Save data successfully";
                }
                else
                {
                    message = "Save data failed.";
                }
            }
            return Json(message, "application/json", Encoding.UTF8, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SelectQuizz()
        {
            if (Session["UserConnected"] != null)
            {
                QuizVM quiz = new viewModels.QuizVM();
                quiz.ListOfQuizz = dbContext.Quizs.Select(q => new SelectListItem
                {
                    Text = q.QuizName,
                    Value = q.QuizID.ToString()

                }).ToList();

                return View(quiz);
            }
            return RedirectToAction("GetUser");
        }

        [HttpPost]
        public ActionResult SelectQuizz(QuizVM quiz)
        {
            QuizVM quizSelected = dbContext.Quizs.Where(q => q.QuizID == quiz.QuizID).Select(q => new QuizVM
            {
                QuizID = q.QuizID,
                QuizName = q.QuizName,

            }).FirstOrDefault();

            if (quizSelected != null)
            {
                Session["SelectedQuiz"] = quizSelected;

                return RedirectToAction("QuizTest");
            }

            return View();
        }

        [HttpGet]
        public ActionResult QuizTest()
        {
            if (Session["UserConnected"] != null)
            {
                QuizVM quizSelected = Session["SelectedQuiz"] as QuizVM;
                IQueryable<QuestionVM> questions = null;

                if (quizSelected != null)
                {
                    questions = dbContext.Questions.Where(q => q.Quiz.QuizID == quizSelected.QuizID)
                       .Select(q => new QuestionVM
                       {
                           QuestionID = q.QuestionID,
                           QuestionText = q.QuestionText,
                           Choices = q.Choices.Select(c => new ChoiceVM
                           {
                               ChoiceID = c.ChoiceID,
                               ChoiceText = c.ChoiceText
                           }).ToList()

                       }).AsQueryable();
                }
                return View(questions);
            }
            return RedirectToAction("GetUser");
        }

        [HttpPost]
        public ActionResult QuizTest(List<QuizAnswersVM> resultQuiz)
        {
            List<QuizAnswersVM> finalResultQuiz = new List<viewModels.QuizAnswersVM>();

            foreach (QuizAnswersVM answser in resultQuiz)
            {
                QuizAnswersVM result = dbContext.Answers.Where(a => a.QuestionID == answser.QuestionID).Select(a => new QuizAnswersVM
                {
                    QuestionID = a.QuestionID.Value,
                    AnswerQ = a.AnswerText,
                    isCorrect = (answser.AnswerQ.ToLower().Equals(a.AnswerText.ToLower()))

                }).FirstOrDefault();

                finalResultQuiz.Add(result);
            }

            return Json(new { result = finalResultQuiz }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LogOut()
        {
            Session.Clear();
            return RedirectToAction("GetUser");
        }
    }
}