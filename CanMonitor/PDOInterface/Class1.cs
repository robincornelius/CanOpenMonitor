using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libCanopenSimple;

namespace PDOInterface
{
    public interface IPDOParser
    {
        void registerPDOS(Dictionary<UInt16, Func<byte[], string>> dic);
        string decodesdo(int node, int index, int sub, byte[] payload);
        void setlco(libCanopen lco);

    }

    public interface IInterfaceService
    {
        IVerb[] GetVerbs(string category);
    }


    public interface IVerb
    {
        string Category { get; }
        string Name { get; }
        void Action(object sender, System.EventArgs e);
    }

    public class InterfaceService: IInterfaceService
    {

        Dictionary<string, List<IVerb>> verbs = new Dictionary<string, List<IVerb>>();

        public IVerb[] GetVerbs(string category)
        {
            if (verbs.ContainsKey(category))
                return verbs[category].ToArray();
            else
                return null;
        }

        protected void addverb(string name, string category, Action<object, System.EventArgs> action)
        {
            verb v = new verb(name, category, action);

            if (!verbs.ContainsKey(category))
                verbs.Add(category, new List<IVerb>());

            verbs[category].Add(v);

        }
    }

    public class verb : IVerb
    {
        private string _category;
        private string _name;
        Action<object, System.EventArgs> _action;

        public verb(string name,string category, Action<object, System.EventArgs> action)
        {
            _action = action;
            _category = category;
            _name = name;
        }

        public string Category
        {
            get { return this._category; }
        }

        public string Name
        {
            get { return this._name; }
        }

        public void Action(object sender, System.EventArgs e)
        {
            if(_action!=null)
                _action(sender,e);
        }

    }
}
