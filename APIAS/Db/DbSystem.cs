using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;

namespace APIAS.Db
{
    class DbSystem
    {
        public RethinkDB R1 { get; }

        private Connection _conn;
        private const string _dbName = "ApiasDb";

        private const string _guildTableName = "Guilds";

        public DbSystem()
        {
            R1 = RethinkDB.R;
        }

        public async Task InitAsync()
            => await InitAsync(_dbName);

        public async Task InitAsync(string DbName)
        {
            _conn = await R1.Connection().ConnectAsync();
            if (!await R1.DbList().Contains(DbName).RunAsync<bool>(_conn)) {
                R1.DbCreate(DbName).Run(_conn);
            }
            if (!await R1.Db(DbName).TableList().Contains(_guildTableName).RunAsync<bool>(_conn)) {
                R1.Db(DbName).TableCreate(_guildTableName).Run(_conn);
            }
        }

        public async Task InitGuildAsync(IGuild guild)
        {
            string GuildID = guild.Id.ToString();
            if (await R1.Db(_dbName).Table(_guildTableName).GetAll(GuildID).Count().Eq(0).RunAsync<bool>(_conn)) {
                Server server = new Server
                {
                    id = GuildID,
                    ServerName = guild.Name,
                    YTFollows = new List<YTFollow>()
                };

                await R1.Db(_dbName).Table(_guildTableName).Insert(server).RunAsync(_conn);
            }
        }
    }
}
