using System.Collections.Generic;
using System.Threading.Tasks;
using Lame.DomainModel;

namespace Lame.Frontend.ViewModels.Imports;

public interface IImportViewModel
{
    Task<List<ImportData>> GetImportData(string fileName);
}