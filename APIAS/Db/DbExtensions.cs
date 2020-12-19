using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;

namespace APIAS.Db
{
    public static class DbExtensions
    {
        public static async Task<bool> CreateIfNotExist(this RethinkDB Db, Connection conn, string DbName, string Table = null)
        {
            if (Table == null) {
                if (!await Db.DbList().Contains(DbName).RunAsync<bool>(conn))
                    Db.DbCreate(DbName);
                return false;
            }

            if (!await Db.Db(DbName).TableList().Contains(Table).RunAsync<bool>(conn)) {
                await Db.Db(DbName).TableCreate(Table).RunAsync(conn);
                return false;
            }

            return true;
        }
    }
}
