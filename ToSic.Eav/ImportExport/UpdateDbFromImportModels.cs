using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Import;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.ImportExport
{
    class UpdateDbFromImportModels: DbExtensionCommandsBase
    {
        public UpdateDbFromImportModels(EavContext cntx) : base(cntx)
        {
        }

    }
}
