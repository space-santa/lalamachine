using System.Collections.Generic;

namespace LibLala.DomainPrimitives
{
    public class GenreList : BaseNameStringList<GenreName>
    {
        public GenreList(List<string> nameStrings) : base(nameStrings) { }
        public GenreList(string nameCsvString) : base(nameCsvString) { }
    }
}
