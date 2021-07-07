
using System.Collections.Generic;

namespace CCAPIProject.Models
{
    public class Table
    {
        private List<Attribute> a = new List<Attribute>();
        public string tableName{ get; set; }
        public string hashKey { get; set; }
        public string rangeKey { get; set; }
        public List<Attribute> attr { get{return this.a;} set{a=value;} }
    }
}