using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToSic.Eav.Import;

namespace ToSic.Eav.Persistence
{
    public class DbValueCommands
    {
        public EavContext Context { get; private set; }

        public DbValueCommands(EavContext cntxt)
        {
            Context = cntxt;
        }


        /// <summary>
        /// Copy all Values (including Related Entities) from teh Source Entity to the target entity
        /// </summary>
        internal void CloneEntityValues(Entity source, Entity target)
        {
            // Clear values on target (including Dimensions). Must be done in separate steps, would cause unallowed null-Foreign-Keys
            if (target.Values.Any())
            {
                foreach (var eavValue in target.Values)
                    eavValue.ChangeLogIDDeleted = Context.Versioning.GetChangeLogId();
            }

            // Add all Values with Dimensions
            foreach (var eavValue in source.Values.ToList())
            {
                var value = eavValue.CopyEntity(Context);
                // copy Dimensions
                foreach (var valuesDimension in eavValue.ValuesDimensions)
                    value.ValuesDimensions.Add(new ValueDimension
                    {
                        DimensionID = valuesDimension.DimensionID,
                        ReadOnly = valuesDimension.ReadOnly
                    });

                target.Values.Add(value);
            }

            target.EntityParentRelationships.Clear();
            // Add all Related Entities
            foreach (var entityParentRelationship in source.EntityParentRelationships)
                target.EntityParentRelationships.Add(new EntityRelationship
                {
                    AttributeID = entityParentRelationship.AttributeID,
                    ChildEntityID = entityParentRelationship.ChildEntityID
                });
        }

        /// <summary>
        /// Add a new Value
        /// </summary>
        internal EavValue AddValue(Entity entity, int attributeId, string value, bool autoSave = true)
        {
            var changeId = Context.Versioning.GetChangeLogId();

            var newValue = new EavValue
            {
                AttributeID = attributeId,
                Entity = entity,
                Value = value,
                ChangeLogIDCreated = changeId
            };

            Context.AddToValues(newValue);
            if (autoSave)
                Context.SaveChanges();
            return newValue;
        }


        /// <summary>
        /// Update a Value
        /// </summary>
        internal void UpdateValue(EavValue currentValue, string value, int changeId, bool autoSave = true)
        {
            // only if value has changed
            if (currentValue.Value.Equals(value))
                return;

            currentValue.Value = value;
            currentValue.ChangeLogIDModified = changeId;
            currentValue.ChangeLogIDDeleted = null;

            if (autoSave)
                Context.SaveChanges();
        }



    }
}
