using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUIComponent
{
    public partial class IndButton : UserControl
    {

        public delegate void NotifyDown();  // delegate
        public delegate void NotifyUp();  // delegate

        public event NotifyDown OnNotifyDown; // event
        public event NotifyUp OnNotifyUp; // event


        public enum Ebuttontype
        {
            BUTTON_RED,
            BUTTON_GREEN,
            BUTTON_BLUE,
            LAMP_GREEN,
            LAMP_RED,
            LAMP_BLUE,
        }

        public enum Elampstate
        {
            ON,
            OFF,
        }

        public enum EButtonState
        {
            UP,
            DOWN
        }

        Ebuttontype _buttontype;

        public Ebuttontype ButtonType {get {return _buttontype; } set { _buttontype = value; render();}}


        Elampstate _lampstate;

        public Elampstate LampState { get { return _lampstate; } set { _lampstate = value; render(); }}


        EButtonState _buttonstate;
        public EButtonState ButtonState { get { return _buttonstate; } set { _buttonstate = value; render(); } }


        public IndButton()
        {
            InitializeComponent();

            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;

        }



        void render()
        {

       
            switch (LampState)
            {

                case Elampstate.OFF:

                    switch (ButtonType)
                    {

                        case Ebuttontype.BUTTON_RED:
                            if(ButtonState == EButtonState.UP)
                                this.pictureBox1.Image = GUIComponents.Properties.Resources.buttonredoffup;
                            else
                                this.pictureBox1.Image = GUIComponents.Properties.Resources.buttonredoffdown;
                            break;

                        case Ebuttontype.BUTTON_GREEN:
                            if (ButtonState == EButtonState.UP)
                                this.pictureBox1.Image = GUIComponents.Properties.Resources.buttongreenoffup;
                            else
                                this.pictureBox1.Image = GUIComponents.Properties.Resources.buttongreenoffdown;
                            break;
                          

                        case Ebuttontype.BUTTON_BLUE:
                            if (ButtonState == EButtonState.UP)
                                this.pictureBox1.Image = GUIComponents.Properties.Resources.buttonblueoffup;
                            else
                                this.pictureBox1.Image = GUIComponents.Properties.Resources.buttonblueoffdown;
                            break;
                           



                        case Ebuttontype.LAMP_RED:
                            this.pictureBox1.Image = GUIComponents.Properties.Resources.lampredoff;
                            break;

                        case Ebuttontype.LAMP_GREEN:
                            this.pictureBox1.Image = GUIComponents.Properties.Resources.lampgreenoff;
                            break;


                        case Ebuttontype.LAMP_BLUE:
                            this.pictureBox1.Image = GUIComponents.Properties.Resources.lampblueoff;
                            break;
                    }

                    break;

                case Elampstate.ON:
                    switch (ButtonType)
                    {

                        case Ebuttontype.BUTTON_RED:
                            if (ButtonState == EButtonState.UP)
                                this.pictureBox1.Image = GUIComponents.Properties.Resources.buttonredonup;
                            else
                                this.pictureBox1.Image = GUIComponents.Properties.Resources.buttonredondown;
                            
                            break;

                        case Ebuttontype.BUTTON_GREEN:
                            if (ButtonState == EButtonState.UP)
                                this.pictureBox1.Image = GUIComponents.Properties.Resources.buttongreenonup;
                            else
                                this.pictureBox1.Image = GUIComponents.Properties.Resources.buttongreenondown;
                            break;
                         

                        case Ebuttontype.BUTTON_BLUE:
                            if (ButtonState == EButtonState.UP)
                                this.pictureBox1.Image = GUIComponents.Properties.Resources.buttonblueonup;
                            else
                                this.pictureBox1.Image = GUIComponents.Properties.Resources.buttonblueoffdown;
                            break;


                        case Ebuttontype.LAMP_RED:
                            this.pictureBox1.Image = GUIComponents.Properties.Resources.lampredon;
                            break;

                        case Ebuttontype.LAMP_GREEN:
                            this.pictureBox1.Image = GUIComponents.Properties.Resources.lampgreenon;
                            break;
                    }

                    break;


            }

        }

      
        public void SetLampState(bool on)
        {
            if (on == true)
                LampState = Elampstate.ON;
            else
                LampState = Elampstate.OFF;

        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            ButtonState = EButtonState.UP;
            render();
            OnNotifyUp?.Invoke();
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            ButtonState = EButtonState.DOWN;
            render();
            OnNotifyDown?.Invoke();
        }

   
    }
}
