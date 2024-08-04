using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LibLala.DomainPrimitives
{
    public class GenreList : BaseNameStringList<GenreName>
    {
        public GenreList(IList<string> nameStrings) : base(nameStrings) { }
        public GenreList(string nameCsvString) : base(nameCsvString) { }
    }
}
