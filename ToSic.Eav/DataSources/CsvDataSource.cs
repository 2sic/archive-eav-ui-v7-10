using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic.FileIO;


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

        public int IdColumnIndex
        {
            get { return int.Parse(Configuration[IdColumnIndexKey]); }
            set { Configuration[IdColumnIndexKey] = value.ToString(); }
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
            Configuration.Add(IdColumnIndexKey, "[Settings:IdColumnIndex||0]");
            Configuration.Add(TitleColumnIndexKey, "[Settings:TitleColumnIndex||0]");
            CacheRelevantConfigurations = new[] { FilePathKey, DelimiterKey, ContentTypeKey };
        }


        private IEnumerable<IEntity> GetList()
        {
            EnsureConfigurationIsLoaded();

            var entityList = new List<IEntity>();

            using (var parser =  new TextFieldParser(FilePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.TrimWhiteSpace = true;
                parser.Delimiters = new string[] { Delimiter };

                // Parse header
                var columns = parser.ReadFields();
                if (IdColumnIndex < 0 || IdColumnIndex >= columns.Length)
                    throw new ArgumentException("Index for ID column is out of range.", "IdColumnIndex");

                if (TitleColumnIndex < 0 || TitleColumnIndex >= columns.Length)
                    throw new ArgumentException("Index for Title column is out of range.", "TitleColumnIndex");

                // Parse data
                while (!parser.EndOfData) 
                {
                    var fields = parser.ReadFields();
                    if (fields.Length != columns.Length)
                        throw new FormatException("Row " + parser.LineNumber + ": The number of fields does not match the column count.");

                    int entityId;
                    if (!int.TryParse(fields[IdColumnIndex], out entityId))
                    {
                        entityId = (int)parser.LineNumber;
                    }
                    var entityValues = new Dictionary<string, object>();
                    for (var i = 0; i < columns.Length; i++)
                    {
                        entityValues.Add(columns[i], fields[i]);
                    }

                    entityList.Add(new Data.Entity(entityId, ContentType, entityValues, columns[TitleColumnIndex]));
                }
            }
            return entityList;
        }
    }
}