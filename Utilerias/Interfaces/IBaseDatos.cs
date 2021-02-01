using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilerias.Interfaces
{
    public interface IBaseDatos
    {
        T SelectFirst<T>(string query);
        Task<T> SelectFirstAsync<T>(string query);
        List<T> Select<T>(string query);
        Task<List<T>> SelectAsync<T>(string query);
        bool Insert(string query);
        Task<bool> InsertAsync(string query);
        bool Update(string query);
        Task<bool> UpdateAsync(string query);
        bool Delete(string query);
        Task<bool> DeleteAsync(string query);
    }
}
