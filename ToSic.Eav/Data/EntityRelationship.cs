using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents Relationships to Child Entities
    /// </summary>
    public class EntityRelationship : IEnumerable<IEntity>
    {
        private static readonly int[] EntityIdsEmpty = new int[0];
        /// <summary>
        /// List of Child EntityIds
        /// </summary>
        public IEnumerable<int> EntityIds { get; internal set; }

        private readonly IDataSource _source;
        //private EntityEnum _entityEnum;
        private List<IEntity> _entities;

        /// <summary>
        /// Initializes a new instance of the EntityRelationship class.
        /// </summary>
        /// <param name="source">DataSource to retrieve child entities</param>
        public EntityRelationship(IDataSource source)
        {
            _source = source;
            EntityIds = EntityIdsEmpty;
        }

        public override string ToString()
        {
            return EntityIds == null ? string.Empty : string.Join(", ", EntityIds.Select(e => e));
        }

        public IEnumerator<IEntity> GetEnumerator()
        {
            if (_entities == null)
                //_entities = _source == null ? new List<IEntity>() : _source.Out[DataSource.DefaultStreamName].List.Where(l => EntityIds.Contains(l.Key)).Select(l => l.Value).ToList();
                _entities = _source == null ? new List<IEntity>() : EntityIds.Select(l => _source.Out[DataSource.DefaultStreamName].List[l]).ToList();

            return new EntityEnum(_entities);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <remarks>Source: http://msdn.microsoft.com/en-us/library/system.collections.ienumerable.getenumerator.aspx </remarks>
        class EntityEnum : IEnumerator<IEntity>
        {
            private readonly List<IEntity> _entities;
            private int _position = -1;

            public EntityEnum(List<IEntity> entities)
            {
                _entities = entities;
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                _position++;
                return (_position < _entities.Count);
            }

            public void Reset()
            {
                _position = -1;
            }

            public IEntity Current
            {
                get
                {
                    try
                    {
                        return _entities[_position];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        throw new InvalidOperationException();
                    }
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
    }
}