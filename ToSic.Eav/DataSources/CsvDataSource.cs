using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using System.IO;


namespace ToSic.Eav.DataSources
{
    [PipelineDesigner]
    public class CsvDataSource : BaseDataSource
    {
        private const string FilePathKey = "FilePath";

        public string FilePath
        {
            get { return Configuration[FilePathKey]; }
            set { Configuration[FilePathKey] = value; }
        }


        private const string DelimiterKey = "Delimiter";

        public string Delimiter
        {
            get { return Configuration[DelimiterKey]; }
            set { Configuration[DelimiterKey] = value; }
        }


        private const string ContentTypeKey = "ContentType";

        public string ContentType
        {
            get { return Configuration[ContentTypeKey]; }
            set { Configuration[ContentTypeKey] = value; }
        }


        private const string IdColumnIndexKey = "IdColumnIndex";

        public int? IdColumnIndex
        {
            get
            {
                var value = Configuration[IdColumnIndexKey];
                if (string.IsNullOrEmpty(value))
                {
                    return null;
                }
                return new int?(int.Parse(value));
            }
            set 
            { 
                Configuration[IdColumnIndexKey] = value == null ? null : value.ToString();
            }
                
        }


        private const string TitleColumnIndexKey = "TitleColumnIndex";

        public int TitleColumnIndex
        {
            get { return int.Parse(Configuration[TitleColumnIndexKey]); }
            set { Configuration[TitleColumnIndexKey] = value.ToString(); }
        }


        public CsvDataSource()
        {
            Out.Add(DataSource.DefaultStreamName, new DataStream(this, DataSource.DefaultStreamName, null, GetList));

            Configuration.Add(FilePathKey, "[Settings:FilePath]");
            Configuration.Add(DelimiterKey, "[Settings:Delimiter||;]");
            Configuration.Add(ContentTypeKey, "[Settings:ContentType||Anonymous]");
            Configuration.Add(IdColumnIndexKey, "[Settings:IdColumnIndex]");
            Configuration.Add(TitleColumnIndexKey, "[Settings:TitleColumnIndex||0]");
            CacheRelevantConfigurations = new[] { FilePathKey, DelimiterKey, ContentTypeKey };
        }


        private IEnumerable<IEntity> GetList()
        {
            EnsureConfigurationIsLoaded();

            var entityList = new List<IEntity>();

            using (var stream = new StreamReader(FilePath))
            using (var parser = new CsvReader(stream))
            {
                parser.Configuration.Delimiter = Delimiter;
                parser.Configuration.HasHeaderRecord = true;
                parser.Configuration.TrimHeaders = true;
                parser.Configuration.TrimFields = true;

                // Parse data
                while (parser.Read())
                {
                    var fields = parser.CurrentRecord;

                    int entityId;
                    if (!IdColumnIndex.HasValue)
                    {   // No ID column specified, so use the line number
                        entityId = parser.Row;
                    }
                    else if (IdColumnIndex.Value < 0 || IdColumnIndex.Value >= parser.FieldHeaders.Length)
                    {
                        throw new ArgumentException("Index for ID column is out of range.", "IdColumnIndex");
                    }
                    else if (!int.TryParse(fields[IdColumnIndex.Value], out entityId))
                    {
                        throw new FormatException("Row " + parser.Row + ": The ID field '" + fields[IdColumnIndex.Value] + "' cannot be parsed.");
                    }

                    if (TitleColumnIndex < 0 || TitleColumnIndex >= parser.FieldHeaders.Length)
                    {
                        throw new ArgumentException("Index for Title column is out of range.", "TitleColumnIndex");
                    }

                    var entityValues = new Dictionary<string, object>();
                    for (var i = 0; i < parser.FieldHeaders.Length; i++)
                    {
                        entityValues.Add(parser.FieldHeaders[i], fields[i]);
                    }

                    entityList.Add(new Data.Entity(entityId, ContentType, entityValues, parser.FieldHeaders[TitleColumnIndex]));
                }
            }
            return entityList;
        }
    }
}