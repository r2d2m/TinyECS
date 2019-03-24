﻿using System;
using System.Collections.Generic;
using TinyECS.Interfaces;


namespace TinyECS.Impls
{
    /// <summary>
    /// class ComponentManager
    /// 
    /// The class is an implementation of a component manager
    /// </summary>

    public class ComponentManager: IComponentManager
    {
        protected IDictionary<uint, IDictionary<Type, int>> mEntity2ComponentsHashTable;

        protected IDictionary<Type, int>                    mComponentTypesHashTable;

        protected IList<IList<IComponent>>                  mComponentsMatrix;

        public ComponentManager()
        {
            mEntity2ComponentsHashTable = new Dictionary<uint, IDictionary<Type, int>>();

            mComponentsMatrix = new List<IList<IComponent>>();

            mComponentTypesHashTable = new Dictionary<Type, int>();
        }

        /// <summary>
        /// The method attaches a new component to the entity
        /// </summary>
        /// <param name="entityId">Entity's identifier</param>
        /// <param name="componentInitializer">A type's value that is used to initialize fields of a new component</param>
        /// <typeparam name="T">A type of a component that should be attached</typeparam>

        public void AddComponent<T>(uint entityId, T componentInitializer) where T : struct, IComponent
        {
            // check does the entity have the corresponding component already
            if (!mEntity2ComponentsHashTable.ContainsKey(entityId))
            {
                // there is no an entity with corresponding id in the table
                mEntity2ComponentsHashTable.Add(entityId, new Dictionary<Type, int>());
            }

            var entityComponentsTable = mEntity2ComponentsHashTable[entityId];
            
            Type cachedComponentType = typeof(T);
            
            // create a new group of components if it doesn't exist yet
            if (!mComponentTypesHashTable.ContainsKey(cachedComponentType))
            {
                mComponentTypesHashTable.Add(cachedComponentType, mComponentsMatrix.Count);

                mComponentsMatrix.Add(new List<IComponent>()); // add a list for the new group
            }

            int componentGroupHashValue = mComponentTypesHashTable[cachedComponentType];

            var componentsGroup = mComponentsMatrix[componentGroupHashValue];

            /// update internal value of the component if it already exists
            if (entityComponentsTable.ContainsKey(cachedComponentType))
            {
                componentsGroup[entityComponentsTable[cachedComponentType]] = componentInitializer;

                return;
            }

            // create a new component
            entityComponentsTable.Add(cachedComponentType, componentsGroup.Count);

            componentsGroup.Add(componentInitializer);
        }

        /// <summary>
        /// The method returns a component of a given type if it belongs to
        /// the specified entity
        /// </summary>
        /// <typeparam name="T">A type of a component that should be retrieved</typeparam>
        /// <param name="entityId">Entity's identifier</param>
        /// <returns>The method returns a component of a given type if it belongs to
        /// the specified entity</returns>

        public T GetComponent<T>(uint entityId) where T : struct, IComponent
        {
            /// there is no entity with the specified id
            if (!mEntity2ComponentsHashTable.ContainsKey(entityId))
            {
                throw new EntityDoesntExistException(entityId);
            }

            var entityComponentsTable = mEntity2ComponentsHashTable[entityId];

            /// there is no component with specified type that belongs to the entity
            Type cachedComponentType = typeof(T);

            if (!entityComponentsTable.ContainsKey(cachedComponentType))
            {
                throw new ComponentDoesntExistException(cachedComponentType, entityId);
            }

            int componentTypeGroupHashValue = mComponentTypesHashTable[cachedComponentType];
            int componentHashValue          = entityComponentsTable[cachedComponentType];


            return (T)mComponentsMatrix[componentTypeGroupHashValue][componentHashValue];
        }

        /// <summary>
        /// The method removes a component of a specified type
        /// </summary>
        /// <param name="entityId">Entity's identifier</param>
        /// <typeparam name="T">A type of a component that should be removed</typeparam>

        public void RemoveComponent<T>(uint entityId) where T : struct, IComponent
        {
            /// there is no entity with the specified id
            if (!mEntity2ComponentsHashTable.ContainsKey(entityId))
            {
                throw new EntityDoesntExistException(entityId);
            }

            var entityComponentsTable = mEntity2ComponentsHashTable[entityId];

            /// there is no component with specified type that belongs to the entity
            Type cachedComponentType = typeof(T);

            if (!entityComponentsTable.ContainsKey(cachedComponentType))
            {
                throw new ComponentDoesntExistException(cachedComponentType, entityId);
            }

            entityComponentsTable.Remove(cachedComponentType);
        }
    }
}
