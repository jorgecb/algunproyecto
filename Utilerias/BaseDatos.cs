using Dapper;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Utilerias.Enum;
using Utilerias.Interfaces;
using static Utilerias.Enum.TipoBaseDatos;

namespace Utilerias
{
    public class BaseDatos : IBaseDatos
    {
        /// <summary>
        /// 
        /// </summary>
        private string _conexiondb;
        private TipoBase _gestordb;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="conexiondb">Cadena de conexión de base de datos</param>
        /// <param name="gestordb">Gestor Base de Datos (MsSql, MySql, Oracle)</param>
        public BaseDatos(string conexiondb, TipoBase gestordb)
        {
            if (string.IsNullOrWhiteSpace(conexiondb))
                throw new ArgumentNullException("'conexiondb' no puede ir nulo o vacío");

            _conexiondb = conexiondb;
            _gestordb = gestordb;

            ValidaCadenaConexion();
        }

        /// <summary>
        /// SQL - SELECT
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">Query SQL</param>
        /// <returns>Resultado consulta</returns>
        public T SelectFirst<T>(string query)
        {
            T respuesta;

            try
            {
                switch (_gestordb)
                {
                    case TipoBase.MsSql:
                        using (var conexion = new SqlConnection(_conexiondb))
                        {
                            conexion.Open();
                            respuesta = conexion.QueryFirst<T>(query);
                        }
                        break;

                    case TipoBase.MySql:
                        using (var conexion = new MySqlConnection(_conexiondb))
                        {
                            conexion.Open();
                            respuesta = conexion.QueryFirst<T>(query);
                        }
                        break;
                    case TipoBase.Oracle:
                        using (var conexion = new OracleConnection(_conexiondb))
                        {
                            conexion.Open();
                            respuesta = conexion.QueryFirst<T>(query);
                        }
                        break;
                    default:
                        respuesta = default;
                        break;
                }
            }
            catch (Exception ex)
            {
                respuesta = default;
            }

            return respuesta;
        }

        /// <summary>
        /// SQL - SELECT - Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">Query SQL</param>
        /// <returns>Resultado consulta</returns>
        public async Task<T> SelectFirstAsync<T>(string query)
        {
            T respuesta;

            try
            {
                switch (_gestordb)
                {
                    case TipoBase.MsSql:
                        using (var conexion = new SqlConnection(_conexiondb))
                        {
                            await conexion.OpenAsync();
                            respuesta = await conexion.QueryFirstAsync<T>(query);
                        }
                        break;

                    case TipoBase.MySql:
                        using (var conexion = new MySqlConnection(_conexiondb))
                        {
                            await conexion.OpenAsync();
                            respuesta = await conexion.QueryFirstAsync<T>(query);
                        }
                        break;
                    case TipoBase.Oracle:
                        using (var conexion = new OracleConnection(_conexiondb))
                        {
                            await conexion.OpenAsync();
                            respuesta = await conexion.QueryFirstAsync<T>(query);
                        }
                        break;
                    default:
                        respuesta = default;
                        break;
                }
            }
            catch (Exception ex)
            {
                respuesta = default;
            }

            return respuesta;
        }

        /// <summary>
        /// SQL - SELECT
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">Query SQL</param>
        /// <returns>Lista resultado de consulta</returns>
        public List<T> Select<T>(string query)
        {
            List<T> respuesta;

            try
            {
                switch (_gestordb)
                {
                    case TipoBase.MsSql:
                        using (var conexion = new SqlConnection(_conexiondb))
                        {
                            conexion.Open();
                            respuesta = (conexion.Query<T>(query)).ToList();
                        }
                        break;

                    case TipoBase.MySql:
                        using (var conexion = new MySqlConnection(_conexiondb))
                        {
                            conexion.Open();
                            respuesta = (conexion.Query<T>(query)).ToList();
                        }
                        break;
                    case TipoBase.Oracle:
                        using (var conexion = new OracleConnection(_conexiondb))
                        {
                            conexion.Open();
                            respuesta = (conexion.Query<T>(query)).ToList();
                        }
                        break;
                    default:
                        respuesta = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// SQL - SELECT - Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">Query SQL</param>
        /// <returns>Lista resultado de consulta</returns>
        public async Task<List<T>> SelectAsync<T>(string query)
        {
            List<T> respuesta;

            try
            {
                switch (_gestordb)
                {
                    case TipoBase.MsSql:
                        using (var conexion = new SqlConnection(_conexiondb))
                        {
                            await conexion.OpenAsync();

                            respuesta = (await conexion.QueryAsync<T>(query)).ToList();
                        }
                        break;

                    case TipoBase.MySql:
                        using (var conexion = new MySqlConnection(_conexiondb))
                        {
                            await conexion.OpenAsync();
                            respuesta = (await conexion.QueryAsync<T>(query)).ToList();
                        }
                        break;
                    case TipoBase.Oracle:
                        using (var conexion = new OracleConnection(_conexiondb))
                        {
                            await conexion.OpenAsync();
                            respuesta = (await conexion.QueryAsync<T>(query)).ToList();
                        }
                        break;
                    default:
                        respuesta = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                respuesta = null;
            }

            return respuesta;
        }

        /// <summary>
        /// SQL - INSERT
        /// </summary>
        /// <param name="query">Query SQL</param>
        /// <returns>True/False</returns>
        public bool Insert(string query)
        {
            bool respuesta = false;

            switch (_gestordb)
            {
                case TipoBase.MsSql:
                    using (var conexion = new SqlConnection(_conexiondb))
                    {
                        conexion.Open();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = conexion.Execute(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;

                case TipoBase.MySql:
                    using (var conexion = new MySqlConnection(_conexiondb))
                    {
                        conexion.Open();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = conexion.Execute(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;
                case TipoBase.Oracle:
                    using (var conexion = new OracleConnection(_conexiondb))
                    {
                        conexion.Open();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = conexion.Execute(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;
                default:
                    respuesta = false;
                    break;
            }

            return respuesta;
        }

        /// <summary>
        /// SQL - INSERT
        /// </summary>
        /// <param name="query">Query SQL</param>
        /// <returns>True/False</returns>
        public async Task<bool> InsertAsync(string query)
        {
            bool respuesta = false;

            switch (_gestordb)
            {
                case TipoBase.MsSql:
                    using (var conexion = new SqlConnection(_conexiondb))
                    {
                        await conexion.OpenAsync();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = await conexion.ExecuteAsync(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;

                case TipoBase.MySql:
                    using (var conexion = new MySqlConnection(_conexiondb))
                    {
                        await conexion.OpenAsync();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = await conexion.ExecuteAsync(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;
                case TipoBase.Oracle:
                    using (var conexion = new OracleConnection(_conexiondb))
                    {
                        await conexion.OpenAsync();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = await conexion.ExecuteAsync(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;
                default:
                    respuesta = false;
                    break;
            }

            return respuesta;
        }

        /// <summary>
        /// SQL - INSERT Masivo
        /// </summary>
        /// <param name="dataTables">Lista de DataTable</param>
        /// <returns>True/False</returns>
        public async Task<bool> InsertBulkAsync(List<DataTable> dataTables)
        {
            bool respuesta = false;

            switch (_gestordb)
            {
                case TipoBase.MsSql:
                    using (var conexion = new SqlConnection(_conexiondb))
                    {
                        await conexion.OpenAsync();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            using (var bulkCopy = new SqlBulkCopy(conexion, SqlBulkCopyOptions.Default, transaccion))
                            {

                                try
                                {
                                    foreach (var dt in dataTables)
                                    {
                                        bulkCopy.DestinationTableName = dt.TableName;

                                        for (int i = 0; i < dt.Columns.Count; i++)
                                        {
                                            var columnName = dt.Columns[i].ColumnName;
                                            bulkCopy.ColumnMappings.Add(columnName, columnName);
                                        }

                                        await bulkCopy.WriteToServerAsync(dt);

                                        respuesta = true;
                                    }

                                    transaccion.Commit();
                                }
                                catch (Exception ex)
                                {
                                    transaccion.Rollback();
                                    respuesta = false;
                                }
                            }
                        }
                    }
                    break;
                default:
                    respuesta = false;
                    break;
            }

            return respuesta;
        }

        /// <summary>
        /// SQL - UPDATE
        /// </summary>
        /// <param name="query">Query SQL</param>
        /// <returns>True/False</returns>
        public bool Update(string query)
        {
            bool respuesta = false;

            switch (_gestordb)
            {
                case TipoBase.MsSql:
                    using (var conexion = new SqlConnection(_conexiondb))
                    {
                        conexion.Open();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = conexion.Execute(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;

                case TipoBase.MySql:
                    using (var conexion = new MySqlConnection(_conexiondb))
                    {
                        conexion.Open();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = conexion.Execute(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;
                case TipoBase.Oracle:
                    using (var conexion = new OracleConnection(_conexiondb))
                    {
                        conexion.Open();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = conexion.Execute(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;
                default:
                    respuesta = false;
                    break;
            }

            return respuesta;
        }

        /// <summary>
        /// SQL - UPDATE - Async
        /// </summary>
        /// <param name="query">Query SQL</param>
        /// <returns>True/False</returns>
        public async Task<bool> UpdateAsync(string query)
        {
            bool respuesta = false;

            switch (_gestordb)
            {
                case TipoBase.MsSql:
                    using (var conexion = new SqlConnection(_conexiondb))
                    {
                        await conexion.OpenAsync();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = await conexion.ExecuteAsync(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;

                case TipoBase.MySql:
                    using (var conexion = new MySqlConnection(_conexiondb))
                    {
                        await conexion.OpenAsync();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = await conexion.ExecuteAsync(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;
                case TipoBase.Oracle:
                    using (var conexion = new OracleConnection(_conexiondb))
                    {
                        await conexion.OpenAsync();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = await conexion.ExecuteAsync(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;
                default:
                    respuesta = false;
                    break;
            }

            return respuesta;
        }

        /// <summary>
        /// SQL - DELETE
        /// </summary>
        /// <param name="query">Query SQL</param>
        /// <returns>True/False</returns>
        public bool Delete(string query)
        {
            bool respuesta = false;

            switch (_gestordb)
            {
                case TipoBase.MsSql:
                    using (var conexion = new SqlConnection(_conexiondb))
                    {
                        conexion.Open();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = conexion.Execute(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;

                case TipoBase.MySql:
                    using (var conexion = new MySqlConnection(_conexiondb))
                    {
                        conexion.Open();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = conexion.Execute(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;
                case TipoBase.Oracle:
                    using (var conexion = new OracleConnection(_conexiondb))
                    {
                        conexion.Open();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = conexion.Execute(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;
                default:
                    respuesta = false;
                    break;
            }

            return respuesta;
        }

        /// <summary>
        /// SQL - DELETE - Async
        /// </summary>
        /// <param name="query">Query SQL</param>
        /// <returns>True/False</returns>
        public async Task<bool> DeleteAsync(string query)
        {
            bool respuesta = false;

            switch (_gestordb)
            {
                case TipoBase.MsSql:
                    using (var conexion = new SqlConnection(_conexiondb))
                    {
                        await conexion.OpenAsync();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = await conexion.ExecuteAsync(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;

                case TipoBase.MySql:
                    using (var conexion = new MySqlConnection(_conexiondb))
                    {
                        await conexion.OpenAsync();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = await conexion.ExecuteAsync(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;
                case TipoBase.Oracle:
                    using (var conexion = new OracleConnection(_conexiondb))
                    {
                        await conexion.OpenAsync();

                        using (var transaccion = conexion.BeginTransaction())
                        {
                            try
                            {
                                var rows = await conexion.ExecuteAsync(query, transaction: transaccion);
                                transaccion.Commit();

                                respuesta = rows > 0 ? true : false;
                            }
                            catch (Exception ex)
                            {
                                transaccion.Rollback();
                                respuesta = false;
                            }
                        }
                    }
                    break;
                default:
                    respuesta = false;
                    break;
            }

            return respuesta;
        }

        private void ValidaCadenaConexion()
        {
            try
            {
                switch (_gestordb)
                {
                    case TipoBase.MsSql:
                        using (var conexion = new SqlConnection(_conexiondb))
                        {
                            conexion.Open();
                        }
                        break;
                    case TipoBase.MySql:
                        using (var conexion = new MySqlConnection(_conexiondb))
                        {
                            conexion.Open();
                        }
                        break;
                    case TipoBase.Oracle:
                        using (var conexion = new OracleConnection(_conexiondb))
                        {
                            conexion.Open();
                        }
                        break;
                    default:
                        throw new ArgumentNullException($"Opción no válida 'TipoBase'");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Cadena de conexión inválida. Excepción: {ex.Message}");
            }
        }
    }
}
