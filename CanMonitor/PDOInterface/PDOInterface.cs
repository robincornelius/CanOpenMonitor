using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using libCanopenSimple;
using WeifenLuo.WinFormsUI.Docking;

namespace PDOInterface
{
    public interface IPDOParser
    {
        void deregisterplugin();
        void registerPDOS();
        string decodesdo(int node, int index, int sub, byte[] payload);

        void setlco(libCanopenSimple.libCanopenSimple lco);

    }

    public interface IInterfaceService
    {
        IVerb[] GetVerbs(string category);
        void setlco(libCanopenSimple.libCanopenSimple lco);
        void preregisterPDOS(Dictionary<UInt16, Func<byte[], string>> dic);
        void deregisterplugin();
        void DriverStateChange(libCanopenSimple.ConnectionChangedEventArgs e);
    }

    public interface IInterfaceService2
    {
        void setdockmanager(DockPanel dp);
    }

    public interface IVerb
    {
        string Category { get; }
        string Name { get; }
        void Action(object sender, System.EventArgs e);
    }

    public class InterfaceService: IInterfaceService
    {
        public libCanopenSimple.libCanopenSimple _lco;
        Dictionary<UInt16, Func<byte[], string>> _dic;
        Dictionary<UInt16, Func<byte[], string>> _dic2;

        public void setlco(libCanopenSimple.libCanopenSimple lco)
        {
            this._lco = lco;
        }

        public void deregisterplugin()
        {
            foreach(KeyValuePair<UInt16, Func<byte[], string>> kvp in _dic2)
            {
                _dic.Remove(kvp.Key);
            }
        }

        public void preregisterPDOS(Dictionary<UInt16, Func<byte[], string>> dic)
        {
            _dic = dic;
            _dic2 = new Dictionary<ushort, Func<byte[], string>>();
            
        }

        public void addpdohook(UInt16 cob, Func<byte[], string> functor)
        {
            _dic.Add(cob, functor);
            _dic2.Add(cob, functor);
        }

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

        public virtual void DriverStateChange(ConnectionChangedEventArgs e)
        {
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
