using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    class SellRoomCommand : IChatCommand
    {
        public string Description
        {
            get { return "Allows the user to sell their own room."; }
        }

        public string Parameters
        {
            get { return "%price%"; }
        }

        public string PermissionRequired
        {
            get { return "command_sell_room"; }
        }

        public void Execute(GameClient Session, Room Room, string[] Params)
        {
            if (!Room.CheckRights(Session, true))
                return;

            if (Params.Length == 1)
            {
                Session.SendWhisper("Oops, you forgot to choose a price to sell the room for.");
                return;
            }
            else if (Room.Group != null)
            {
                Session.SendWhisper("Oops, this room has a group. You must delete the group before you can sell the room.");
                return;
            }
            
            if (!int.TryParse(Params[1], out int price))
            {
                Session.SendWhisper("Oops, you've entered an invalid integer.");
                return;
            }

            if (price == 0)
            {
                Session.SendWhisper("Oops, you cannot sell a room for 0 credits.");
                return;
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `rooms` SET `sale_price` = @SalePrice WHERE `id` = @Id LIMIT 1");
                dbClient.AddParameter("SalePrice", price);
                dbClient.AddParameter("Id", Room.Id);
                dbClient.RunQuery();
            }

            Session.SendNotification("Your room is now up for sale. The the current room visitors have been alerted, any item that belongs to you in this room will be transferred to the new owner once purchased. Other items shall be ejected.");

            foreach (RoomUser user in Room.GetRoomUserManager().GetRoomUsers())
            {
                if (user == null || user.GetClient() == null)
                    continue;

                user.GetClient().SendWhisper("Attention! This room has been put up for sale, you can buy it now for " + price + " credits! Use the :buyroom command.");
            }
        }
    }
}
