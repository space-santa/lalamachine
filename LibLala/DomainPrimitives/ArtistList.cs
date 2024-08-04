using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LibLala.DomainPrimitives
{
    public class ArtistList : BaseNameStringList<ArtistName>
    {
        public ArtistList(IList<string> artistStrings) : base(artistStrings) { }
        public ArtistList(string artistsCsvString) : base(artistsCsvString) { }
    }
}
