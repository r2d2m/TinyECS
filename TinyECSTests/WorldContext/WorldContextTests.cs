﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using TinyECS.Impls;
using TinyECS.Interfaces;


namespace TinyECSTests
{
    /// <summary>
    /// class WorldContextTests
    /// 
    /// The class is a union of integration tests which are related with
    /// WorldContext class and its usage
    /// </summary>

    [TestFixture]
    public class WorldContextTests
    {
        protected struct TTestComponent: IComponent
        {
        }

        protected struct TAnotherComponent : IComponent
        {
        }

        protected struct TUniqueComponent: IUniqueComponent
        {
        }

        protected IEntityManager mEntityManager;

        protected IWorldContext  mWorldContext;

        [SetUp]
        public void Init()
        {
            mEntityManager = new EntityManager(new ComponentManager(new EventManager()));

            mWorldContext = new WorldContext(mEntityManager);
        }

        [Test]
        public void TestEntitiesCreation_Pass()
        {
            // new entities that are created in continuous manner
            // should have increasing identifiers with a delta equals to 1
            for (uint i = 0; i < 10; ++i)
            {
                Assert.AreEqual(i, (uint)mWorldContext.CreateEntity());
                Assert.AreEqual(i, (uint)mWorldContext.GetEntityById((EntityId)i).Id);
            }
        }

        [Test]
        public void TestGetEntitiesWithAll_PassNoArguments_ReturnsAllEntities()
        {
            List<EntityId> expectedEntities = new List<EntityId>();

            for (int i = 0; i < 5; ++i)
            {
                expectedEntities.Add(mWorldContext.CreateEntity());
            }

            Assert.DoesNotThrow(() =>
            {
                var actualEntities = mWorldContext.GetEntitiesWithAll();

                Assert.AreEqual(expectedEntities.Count, actualEntities.Count);
                
                for (int i = 0; i < expectedEntities.Count; ++i)
                {
                    Assert.AreEqual(expectedEntities[i], actualEntities[i]);
                }
            });
        }

        [Test]
        public void TestGetEntitiesWithAll_PassSingleArgument_ReturnsEntitiesWithGivenComponent()
        {
            List<EntityId> expectedEntities = new List<EntityId>(); // contains only entities with TTestComponent's attached to them

            Random randomGenerator = new Random();

            for (int i = 0; i < 5; ++i)
            {
                var entity = mWorldContext.GetEntityById(mWorldContext.CreateEntity());

                if (randomGenerator.Next(0, 2) > 0)
                {
                    entity.AddComponent<TTestComponent>();

                    expectedEntities.Add(entity.Id);
                }
            }

            Assert.DoesNotThrow(() =>
            {
                var actualEntities = mWorldContext.GetEntitiesWithAll(typeof(TTestComponent));

                Assert.AreEqual(expectedEntities.Count, actualEntities.Count);
                
                for (int i = 0; i < expectedEntities.Count; ++i)
                {
                    Assert.AreEqual(expectedEntities[i], actualEntities[i]);
                }
            });
        }

        [Test]
        public void TestGetEntitiesWithAll_PassFewArgument_ReturnsEntitiesThatHaveAllGivenComponents()
        {
            List<EntityId> expectedEntities = new List<EntityId>(); // contains only entities with TTestComponent's attached to them

            Random randomGenerator = new Random();

            for (int i = 0; i < 5; ++i)
            {
                var entity = mWorldContext.GetEntityById(mWorldContext.CreateEntity());

                if (randomGenerator.Next(0, 2) > 0)
                {
                    entity.AddComponent<TTestComponent>();
                    entity.AddComponent<TAnotherComponent>();

                    expectedEntities.Add(entity.Id);
                }
            }

            Assert.DoesNotThrow(() =>
            {
                var actualEntities = mWorldContext.GetEntitiesWithAll(typeof(TTestComponent), typeof(TAnotherComponent));

                Assert.AreEqual(expectedEntities.Count, actualEntities.Count);
                
                for (int i = 0; i < expectedEntities.Count; ++i)
                {
                    Assert.AreEqual(expectedEntities[i], actualEntities[i]);
                }
            });
        }

        [Test]
        public void TestGetEntitiesWithAny_PassNoArguments_ReturnsEmptyArray()
        {
            List<EntityId> expectedEntities = new List<EntityId>();

            // create new entities;
            for (int i = 0; i < 5; ++i)
            {
                var entity = mWorldContext.GetEntityById(mWorldContext.CreateEntity());

                entity.AddComponent<TTestComponent>();
                entity.AddComponent<TAnotherComponent>();
            }

            Assert.DoesNotThrow(() =>
            {
                var actualEntities = mWorldContext.GetEntitiesWithAny();

                Assert.AreEqual(expectedEntities.Count, actualEntities.Count);
            });
        }

        [Test]
        public void TestGetEntitiesWithAny_PassTwoArgument_ReturnsEntitiesWithFirstOrSecondComponent()
        {
            List<EntityId> expectedEntities = new List<EntityId>();

            // create new entities;
            for (int i = 0; i < 5; ++i)
            {
                var entity = mWorldContext.GetEntityById(mWorldContext.CreateEntity());

                expectedEntities.Add(entity.Id);

                entity.AddComponent<TTestComponent>();
                entity.AddComponent<TAnotherComponent>();
            }

            Assert.DoesNotThrow(() =>
            {
                var actualEntities = mWorldContext.GetEntitiesWithAny(typeof(TTestComponent), typeof(TAnotherComponent));

                Assert.AreEqual(expectedEntities.Count, actualEntities.Count);

                for (int i = 0; i < expectedEntities.Count; ++i)
                {
                    Assert.AreEqual(expectedEntities[i], actualEntities[i]);
                }
            });
        }

        [Test]
        public void TestCreateEntity_CreateEntityDeleteItAndRecreateAgain_NewCreatedEntityHasNoComponents()
        {
            // Test to fix issue #18 https://github.com/bnoazx005/TinyECS/issues/18

            EntityId entityId = mWorldContext.CreateEntity();
            IEntity entity = mWorldContext.GetEntityById(entityId);

            Assert.IsNotNull(entity);

            entity.AddComponent<TTestComponent>();
            entity.AddComponent<TAnotherComponent>();

            Assert.IsTrue(mWorldContext.DestroyEntity(entityId));

            // recreate entity
            entityId = mWorldContext.CreateEntity();
            entity = mWorldContext.GetEntityById(entityId);

            // recreated entity should be empty
            Assert.IsNotNull(entity);
            Assert.IsTrue(!entity.HasComponent<TTestComponent>() && !entity.HasComponent<TAnotherComponent>());
        }

        [Test]
        public void TestGetUniqueEntity_TryToGetUnexistingEntity_CreatesNewOneAndReturnsIt()
        {
            // Precondition: There is no entities with TUniqueComponent in the world
            Assert.IsTrue(mWorldContext.GetEntitiesWithAny(typeof(TUniqueComponent)).Count == 0);

            IEntity entity = mWorldContext.GetUniqueEntity<TUniqueComponent>();
            Assert.IsNotNull(entity);
        }

        [Test]
        public void TestGetUniqueEntity_TryToGetAlreadyExistingEntity_ReturnsReferenceToThat()
        {
            IEntity firstEntity = mWorldContext.GetUniqueEntity<TUniqueComponent>();
            Assert.IsNotNull(firstEntity);

            Assert.DoesNotThrow(() =>
            {
                IEntity secondEntity = mWorldContext.GetUniqueEntity<TUniqueComponent>();
                Assert.IsNotNull(secondEntity);
            });
        }

        // The test that reproduce the issue https://github.com/bnoazx005/TinyECS/issues/26

        [Test]
        public void TestGetSingleEntityWithAll_TryToGetSingleEntity_ReturnsThatEntity()
        {
            for (int i = 0; i < 2; ++i)
            {
                mWorldContext.CreateEntity("JunkEntity");
            }

            IEntity expectedEntity = mWorldContext.GetEntityById(mWorldContext.CreateEntity());
            expectedEntity.AddComponent<TTestComponent>();

            IEntity actualEntity = mWorldContext.GetSingleEntityWithAll(typeof(TTestComponent));
            Assert.AreSame(expectedEntity, actualEntity);
        }

        // The test that reproduce the issue https://github.com/bnoazx005/TinyECS/issues/26

        [Test]
        public void TestGetSingleEntityWithAll_TryToGetAllEntitiesWithParticularComponents_ReturnsFirstOne()
        {
            List<IEntity> entities = new List<IEntity>();

            for (int i = 0; i < 3; ++i)
            {
                IEntity entity = mWorldContext.GetEntityById(mWorldContext.CreateEntity());
                entities.Add(entity);

                if (i > 0)
                {
                    entity.AddComponent<TTestComponent>();
                }
            }

            IEntity actualEntity = mWorldContext.GetSingleEntityWithAll(typeof(TTestComponent));
            Assert.IsNotNull(actualEntity);
            Assert.AreSame(entities[1], actualEntity);
        }
    }
}
