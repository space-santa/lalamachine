using LalaDb.Data;

namespace LalaDb.Model
{
    internal class PlaylistModel
    {
        private readonly LalaContext _context;

        public PlaylistModel(LalaContext context)
        {
            _context = context;
        }
    }
}
