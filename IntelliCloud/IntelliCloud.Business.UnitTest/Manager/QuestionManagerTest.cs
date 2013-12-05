﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using nl.fhict.IntelliCloud.Business.Manager;
using nl.fhict.IntelliCloud.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nl.fhict.IntelliCloud.Business.UnitTest.Manager
{
    /// <summary>
     /// In this unit test class all methods of the QuestionManager will be tested.
     /// All these test will use mock classes.
     /// </summary>
     [TestClass]
     public class QuestionManagerTest
     {
         #region Fields
 
         private QuestionManager manager;
         private Mock<IValidation> validation;
 
         #endregion Fields
 
         #region Methods
 
         [TestInitialize]
         public void Initialize()
         {
             validation = new Mock<IValidation>();
             //this.manager = new QuestionManager(null, validation.Object);            
         }
 
         [TestCleanup]
         public void Cleanup()
         {
         }
 
         #region Tests
 
         /// <summary>
         /// In this test we check if the questionId and emplooyeeId is being validated in the UpdateQuestion method.
         /// </summary>
         [TestMethod]
         public void UpdateQuestionTest()
         {
             int questionId = 1;
             int employeeId = 1;
 
             try
             {
                 manager.UpdateQuestion(questionId, employeeId);
             }
             catch (Exception)
             { }

             validation.Verify(v => v.IdCheck(questionId), Times.Once());
             validation.Verify(v => v.IdCheck(employeeId), Times.Once());
         }
 
         /// <summary>
         /// In this test we check if the source, reference, question, title, postId and isPrivate is being validated in the CreateQuestion method.
         /// </summary>
         [TestMethod]
         public void CreateQuestionTest()
         {
             string source = "Mail";
             string reference = "Test@Gmail.coom";
             string question = "This is my question";
             string title ="This is my title";
             string postId = "";
             bool isPrivate =true; 
 
             try
             {
                 manager.CreateQuestion(source, reference, question, title, postId, isPrivate);
             }
             catch (Exception)
             { }

             validation.Verify(v => v.StringCheck(source), Times.Once());
             validation.Verify(v => v.StringCheck(reference), Times.Once());
             validation.Verify(v => v.StringCheck(question), Times.Once());
             validation.Verify(v => v.StringCheck(title), Times.Once());
             validation.Verify(v => v.StringCheck(postId), Times.Once());
             // TODO: boolean check
         }
 
         /// <summary>
         /// In this test we check if the qustionId is being validated in the GetQuestion method.
         /// </summary>
         [TestMethod]
         public void GetQuestionTest()
         {
             int questionId = 1;
 
             try
             {
                 manager.GetQuestion(questionId);
             }
             catch (Exception)
             { }

             validation.Verify(v => v.IdCheck(questionId), Times.Once());
         }
 
         /// <summary>
         /// In this test we check if the employeeId is being validated in the GetQuestions method.
         /// </summary>
         [TestMethod]
         public void GetQuestionsTest()
         {
             int employeeId = 1;
 
             try
             {
                 manager.GetQuestions(employeeId);
             }
             catch (Exception)
             { }
 
             validation.Verify(v => v.IdCheck(employeeId), Times.Once());
         }
 
         #endregion Tests
 
         #endregion Methods
     }


}