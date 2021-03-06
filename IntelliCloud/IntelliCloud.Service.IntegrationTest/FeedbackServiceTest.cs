﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using nl.fhict.IntelliCloud.Common.DataTransfer;
using nl.fhict.IntelliCloud.Data.IntelliCloud.Context;
using nl.fhict.IntelliCloud.Data.IntelliCloud.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace nl.fhict.IntelliCloud.Service.IntegrationTest
{
    /// <summary>
    /// Test class for the service FeedbackService.
    /// </summary>
[TestClass]
    public class FeedbackServiceTest
    {
        #region Fields

        /// <summary>
        /// Instance of class FeedbackService.
        /// </summary>
        private IFeedbackService service;

        /// <summary>
        /// Fields used during testing.
        /// </summary>
        private UserEntity customerEntity;
        private UserEntity employeeEntity;
        private QuestionEntity questionEntity;
        private AnswerEntity answerEntity;
        private FeedbackEntity feedbackEntity;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Initialization method for each test method.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            this.service = new FeedbackService();
            this.initializeTestData();
        }

        /// <summary>
        /// Cleanup method for each test method.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            this.removeTestData();
        }

        /// <summary>
        /// Method that adds test data to the context (used during testing).
        /// </summary>
        private void initializeTestData()
        {
            // First, make sure to remove any existing data
            this.removeTestData();

            // Add test data
            using (IntelliCloudContext context = new IntelliCloudContext())
            {
                // Add a new mail definition
                SourceDefinitionEntity mailSourceDefinition = new SourceDefinitionEntity() { Name = "Mail", CreationTime = DateTime.UtcNow };
                context.SourceDefinitions.Add(mailSourceDefinition);

                // Add a new language definition
                LanguageDefinitionEntity languageDefinition = new LanguageDefinitionEntity() { Name = "English", ResourceName = "EN" };
                context.LanguageDefinitions.Add(languageDefinition);

                // Create a new user entity for a customer
                this.customerEntity = new UserEntity()
                {
                    FirstName = "Name",
                    Infix = "of",
                    LastName = "customer",
                    Type = UserType.Customer,
                    CreationTime = DateTime.UtcNow
                };
                context.Users.Add(this.customerEntity);

                // Create a new source for the customer (email address)
                SourceEntity customerMailSource = new SourceEntity()
                {
                    Value = "customer@domain.com",
                    CreationTime = DateTime.UtcNow,
                    SourceDefinition = mailSourceDefinition,
                    User = this.customerEntity
                };
                context.Sources.Add(customerMailSource);

                // Add a new question
                QuestionSourceEntity questionSource = new QuestionSourceEntity() { Source = customerMailSource };
                context.QuestionSources.Add(questionSource);
                this.questionEntity = new QuestionEntity()
                {
                    Title = "Title of the question",
                    Content = "Content of the question",
                    Source = questionSource,
                    LanguageDefinition = languageDefinition,
                    FeedbackToken = "ABC123",
                    CreationTime = DateTime.UtcNow,
                    User = this.customerEntity
                };
                context.Questions.Add(this.questionEntity);

                // Create a new user entity for an employee
                this.employeeEntity = new UserEntity()
                {
                    FirstName = "Name",
                    Infix = "of",
                    LastName = "employee",
                    Type = UserType.Employee,
                    CreationTime = DateTime.UtcNow
                };
                context.Users.Add(this.customerEntity);

                // Create a new source for the employee (email address)
                SourceEntity employeeMailSource = new SourceEntity()
                {
                    Value = "employee@domain.com",
                    CreationTime = DateTime.UtcNow,
                    SourceDefinition = mailSourceDefinition,
                    User = this.employeeEntity,
                };
                context.Sources.Add(employeeMailSource);

                // Create a new answer
                this.answerEntity = new AnswerEntity()
                {
                    Content = "Content of the answer",
                    LanguageDefinition = languageDefinition,
                    User = this.employeeEntity,
                    CreationTime = DateTime.UtcNow
                };
                context.Answers.Add(this.answerEntity);

                // Set the answer and answerer in the question
                this.questionEntity.Answer = answerEntity;
                this.questionEntity.Answerer = employeeEntity;

                // Add a new feedback entry
                this.feedbackEntity = new FeedbackEntity()
                {
                    Content = "Content of the feedback",
                    FeedbackType = FeedbackType.Declined,
                    FeedbackState = FeedbackState.Open,
                    Answer = this.answerEntity,
                    Question = this.questionEntity,
                    User = this.customerEntity,
                    CreationTime = DateTime.UtcNow
                };
                context.Feedbacks.Add(this.feedbackEntity);

                // Save the changes to the context
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Method that removes test data that was added to the context.
        /// </summary>
        private void removeTestData()
        {
            // Remove all entities from the context
            using (IntelliCloudContext context = new IntelliCloudContext())
            {
                context.Feedbacks.RemoveRange(context.Feedbacks.ToList());
                context.Answers.RemoveRange(context.Answers.ToList());
                context.Questions.RemoveRange(context.Questions.ToList());
                context.QuestionSources.RemoveRange(context.QuestionSources.ToList());
                context.Sources.RemoveRange(context.Sources.ToList());
                context.SourceDefinitions.RemoveRange(context.SourceDefinitions.ToList());
                context.LanguageDefinitions.RemoveRange(context.LanguageDefinitions.ToList());
                context.Users.RemoveRange(context.Users.ToList());

                // Save the changes to the context
                context.SaveChanges();
            }

            // Unset local variables
            customerEntity = null;
            employeeEntity = null;
            questionEntity = null;
            answerEntity = null;
            feedbackEntity = null;
        }

        #region Tests

        /// <summary>
        /// CreateFeedback test method.
        /// </summary>
        [TestMethod]
        public void CreateFeedback()
        {
            try
            {
                // Add a new feedback entry
                string feedback = "Content of the second feedback.";
                int answerId = this.answerEntity.Id;
                int questionId = this.questionEntity.Id;
                FeedbackType feedbackType = FeedbackType.Declined;
                string feedbackToken = "ABC123";
                this.service.CreateFeedback(feedback, answerId, questionId, feedbackType, feedbackToken);

                // Check if the feedback entry was added and contains the correct data
                using (IntelliCloudContext context = new IntelliCloudContext())
                {
                    FeedbackEntity entity = context.Feedbacks
                                            .Include(f => f.Answer)
                                            .Include(f => f.Question)
                                            .Single(f => f.Content.Equals(feedback));

                    Assert.AreEqual(entity.Answer.Id, answerId);
                    Assert.AreEqual(entity.Question.Id, questionId);
                    Assert.AreEqual(entity.FeedbackState, FeedbackState.Open);
                    Assert.AreEqual(entity.FeedbackType, feedbackType);
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        /// <summary>
        /// UpdateFeedback test method.
        /// </summary>
        [TestMethod]
        public void UpdateFeedback()
        {
            try
            {
                string feedbackId = this.feedbackEntity.Id.ToString();
                FeedbackState feedbackState = FeedbackState.Closed;

                this.service.UpdateFeedback(feedbackId, feedbackState);

                using (IntelliCloudContext context = new IntelliCloudContext())
                {
                    FeedbackEntity entity = context.Feedbacks.Single(f => f.Id == this.feedbackEntity.Id);
                    Assert.AreEqual(entity.FeedbackState, feedbackState);
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        /// <summary>
        /// Tests if the GetFeedback is getting a feedback of the specific id, or atleast calls something to the database.
        /// </summary>
        [TestMethod]
        [TestCategory("nl.fhict.IntelliCloud.Service.IntegrationTest")]
        public void GetFeedback()
        {
            try
            {
                string feedbackId = feedbackEntity.Id.ToString();

                var newFeedback = service.GetFeedback(feedbackId);

                Assert.AreEqual(feedbackEntity.Content, newFeedback.Content);
            }
            catch (Exception e) // TODO move exception test to different method, since this allows for skipping a part of the test...
            {
                Assert.AreEqual(e.Message, "Sequence contains no elements");
            }
        }

        /// <summary>
        /// Tests if the GetAnswer is getting an answer of the feedback, or atleast calls something to the database.
        /// </summary>
        [TestMethod]
        [TestCategory("nl.fhict.IntelliCloud.Service.IntegrationTest")]
        public void GetAnswer()
        {
            try
            {
                string feedbackId = feedbackEntity.Id.ToString();

                var newAnswer = service.GetAnswer(feedbackId);

                Assert.AreEqual(answerEntity.Content, newAnswer.Content);
            }
            catch (Exception e) // TODO move exception test to different method, since this allows for skipping a part of the test...
            {
                Assert.AreEqual(e.Message, "Sequence contains no elements");
            }
        }

        /// <summary>
        /// Tests if the GetUser is getting an user of the feedback, or atleast calls something to the database.
        /// </summary>
        [TestMethod]
        [TestCategory("nl.fhict.IntelliCloud.Service.IntegrationTest")]
        public void GetUser()
        {
            try
            {
                string feedbackId = feedbackEntity.Id.ToString();

                var newUser = service.GetUser(feedbackId);

                Assert.AreEqual(employeeEntity.FirstName, newUser.FirstName);

            }
            catch (Exception e) // TODO move exception test to different method, since this allows for skipping a part of the test...
            {
                Assert.AreEqual(e.Message, "Sequence contains no elements");
            }
        }

        /// <summary>
        /// Tests if the GetQuestion is getting the question of the feedback, or atleast calls something to the database.
        /// </summary>
        [TestMethod]
        [TestCategory("nl.fhict.IntelliCloud.Service.IntegrationTest")]
        public void GetQuestion()
        {
            try
            {
                string feedbackId = feedbackEntity.Id.ToString();

                var newQuestion = service.GetQuestion(feedbackId);

                Assert.AreEqual(questionEntity.Content, newQuestion.Content);

            }
            catch (Exception e) // TODO move exception test to different method, since this allows for skipping a part of the test...
            {
                Assert.AreEqual(e.Message, "Sequence contains no elements");
            }
        }

        #endregion Tests

        #endregion Methods
    }
}