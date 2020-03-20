using System;
using PandeaGames.Services;
using Terra.SerializedData.World;
using System.Data.SQLite;

namespace Terra.Services
{
    public class TerraWorldService : IService
    {
        public void LoadWorld(Action<TerraWorld> onComplete, Action<Exception> onError)
        {
            onComplete(new TerraWorld());
            string cs = @"URI=file:C:\Users\Jano\Documents\test.db";

            using (var con = new SQLiteConnection(cs))
            {
                con.Open();
                var cmd = new SQLiteCommand(con);
            }
    
            //TODO: Load Terra World ASYNC
            
        }
    }
}