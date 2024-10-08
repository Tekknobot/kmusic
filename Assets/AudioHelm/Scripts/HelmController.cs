// Copyright 2017 Matt Tytel

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AudioHelm
{
    /// <summary>
    /// ## [Switch to Manual](../manual/class_audio_helm_1_1_helm_controller.html)<br>
    /// An interface to a Helm native synthesizer instance(s).
    /// All native synth settings can be changed through this class.
    /// </summary>
    [RequireComponent(typeof(HelmAudioInit))]
    [AddComponentMenu("Audio Helm/Helm Controller")]
    [HelpURL("http://tytel.org/audiohelm/manual/class_audio_helm_1_1_helm_controller.html")]
    public class HelmController : MonoBehaviour, NoteHandler
    {
        public const float UPDATE_WAIT = 0.04f;
        public const int MAX_PARAMETERS = 100;

        /// <summary>
        /// Specifies which Helm instance(s) to control.
        /// Every Helm instance in any AudioMixerGroup matching this channel number is controlled by this class.
        /// </summary>
        [Tooltip("The native synth channel to send note events to." +
                 " This must match the channel set in the Helm Audio plugin.")]
        public int channel = 0;

        // Note: These parameters listed out to support Unity animations.
        [SerializeField]
        protected float synthParamValue0 = 0.0f, synthParamValue1 = 0.0f, synthParamValue2 = 0.0f, synthParamValue3 = 0.0f,
                        synthParamValue4 = 0.0f, synthParamValue5 = 0.0f, synthParamValue6 = 0.0f, synthParamValue7 = 0.0f,
                        synthParamValue8 = 0.0f, synthParamValue9 = 0.0f, synthParamValue10 = 0.0f, synthParamValue11 = 0.0f,
                        synthParamValue12 = 0.0f, synthParamValue13 = 0.0f, synthParamValue14 = 0.0f, synthParamValue15 = 0.0f,
                        synthParamValue16 = 0.0f, synthParamValue17 = 0.0f, synthParamValue18 = 0.0f, synthParamValue19 = 0.0f,
                        synthParamValue20 = 0.0f, synthParamValue21 = 0.0f, synthParamValue22 = 0.0f, synthParamValue23 = 0.0f,
                        synthParamValue24 = 0.0f, synthParamValue25 = 0.0f, synthParamValue26 = 0.0f, synthParamValue27 = 0.0f,
                        synthParamValue28 = 0.0f, synthParamValue29 = 0.0f, synthParamValue30 = 0.0f, synthParamValue31 = 0.0f,     
                        synthParamValue32 = 0.0f, synthParamValue33 = 0.0f, synthParamValue34 = 0.0f, synthParamValue35 = 0.0f,
                        synthParamValue36 = 0.0f, synthParamValue37 = 0.0f, synthParamValue38 = 0.0f, synthParamValue39 = 0.0f,
                        synthParamValue40 = 0.0f, synthParamValue41 = 0.0f, synthParamValue42 = 0.0f, synthParamValue43 = 0.0f,                                                 
                        synthParamValue44 = 0.0f, synthParamValue45 = 0.0f, synthParamValue46 = 0.0f, synthParamValue47 = 0.0f,
                        synthParamValue48 = 0.0f, synthParamValue49 = 0.0f, synthParamValue50 = 0.0f;

        /// <summary>
        /// List of current parameters you can view, change and animate in the Inspector view.
        /// </summary>
        public List<HelmParameter> synthParameters = new List<HelmParameter>();

        Dictionary<int, int> pressedNotes = new Dictionary<int, int>();
        Dictionary<float, int> pressedFrequencies = new Dictionary<float, int>();

        void OnDestroy()
        {
            AllNotesOff();
        }

        void Awake()
        {
            AllNotesOff();
        }

        void Start()
        {
            Utils.InitAudioSource(GetComponent<AudioSource>());
        }

        /// <summary>
        /// Loads a synthesizer patch at runtime.
        /// </summary>
        /// <param name="patch">Reference to the patch object.</param>
        public void LoadPatch(HelmPatch patch)
        {
            FieldInfo[] fields = typeof(HelmPatchSettings).GetFields();
            Native.HelmClearModulations(channel);

            List<float> values = new List<float>();
            values.Add(0.0f);
            int index = 1;
            foreach (FieldInfo field in fields)
            {
                if (!field.FieldType.IsArray && !field.IsLiteral)
                {
                    float val = (float)field.GetValue(patch.patchData.settings);
                    Native.HelmSetParameterValue(channel, index, val);
                    values.Add(val);
                    index++;
                }
            }

            for (int i = 0; i < synthParameters.Count; ++i)
                SetParameterAtIndex(i, values[(int)synthParameters[i].parameter]);

            int modulationIndex = 0;
            foreach (HelmModulationSetting modulation in patch.patchData.settings.modulations)
            {
                if (modulationIndex >= HelmPatchSettings.kMaxModulations)
                {
                    Debug.LogWarning("Only " + HelmPatchSettings.kMaxModulations +
                                     " modulations are currently supported in the Helm Unity plugin.");
                    break;
                }

                Native.HelmAddModulation(channel, modulationIndex,
                                         modulation.source, modulation.destination, modulation.amount);
                modulationIndex++;
            }
        }

        /// <summary>
        /// Gets the parameter value at index in the parameter list.
        /// </summary>
        /// <param name="index">The index of the parameter to get.</param>
        /// <returns>The value of the parameter.</returns>
        public float GetParameterAtIndex(int index)
        {
            if (index >= synthParameters.Count)
                return 0.0f;

            // Note: These are listed out to support Unity animations.
            switch (index)
            {
                case 0: return synthParamValue0;
                case 1: return synthParamValue1;
                case 2: return synthParamValue2;
                case 3: return synthParamValue3;
                case 4: return synthParamValue4;
                case 5: return synthParamValue5;
                case 6: return synthParamValue6;
                case 7: return synthParamValue7;
                case 8: return synthParamValue8;
                case 9: return synthParamValue9;
                case 10: return synthParamValue10;
                case 11: return synthParamValue11;
                case 12: return synthParamValue12;
                case 13: return synthParamValue13;
                case 14: return synthParamValue14;
                case 15: return synthParamValue15;
                case 16: return synthParamValue16;
                case 17: return synthParamValue17;    
                case 18: return synthParamValue18; 
                case 19: return synthParamValue19; 
                case 20: return synthParamValue20; 
                case 21: return synthParamValue21; 
                case 22: return synthParamValue22; 
                case 23: return synthParamValue23; 
                case 24: return synthParamValue24;   
                case 25: return synthParamValue25;       
                case 26: return synthParamValue26; 
                case 27: return synthParamValue27; 
                case 28: return synthParamValue28; 
                case 29: return synthParamValue29; 
                case 30: return synthParamValue30;   
                case 31: return synthParamValue31;    
                case 32: return synthParamValue32;
                case 33: return synthParamValue33;
                case 34: return synthParamValue34;
                case 35: return synthParamValue35;
                case 36: return synthParamValue36;    
                case 37: return synthParamValue37; 
                case 38: return synthParamValue38; 
                case 39: return synthParamValue39; 
                case 40: return synthParamValue40; 
                case 41: return synthParamValue41; 
                case 42: return synthParamValue42; 
                case 43: return synthParamValue43;   
                case 44: return synthParamValue44;       
                case 45: return synthParamValue45; 
                case 46: return synthParamValue46; 
                case 47: return synthParamValue47; 
                case 48: return synthParamValue48; 
                case 49: return synthParamValue49;   
                case 50: return synthParamValue50;                                                                                                                                                                                  
                default: return 0.0f;
            }
        }

        /// <summary>
        /// Sets the parameter at index in the parameter list to the given value.
        /// </summary>
        /// <param name="index">The index of the parameter to change.</param>
        /// <param name="newValue">The value to change the parameter to.</param>
        public void SetParameterAtIndex(int index, float newValue)
        {
            if (index >= synthParameters.Count)
                return;

            // Note: These are listed out to support Unity animations.
            switch (index)
            {
                case 0: synthParamValue0 = newValue; break;
                case 1: synthParamValue1 = newValue; break;
                case 2: synthParamValue2 = newValue; break;
                case 3: synthParamValue3 = newValue; break;
                case 4: synthParamValue4 = newValue; break;
                case 5: synthParamValue5 = newValue; break;
                case 6: synthParamValue6 = newValue; break;
                case 7: synthParamValue7 = newValue; break;
                case 8: synthParamValue8 = newValue; break;
                case 9: synthParamValue9 = newValue; break;
                case 10: synthParamValue10 = newValue; break;
                case 11: synthParamValue11 = newValue; break;
                case 12: synthParamValue12 = newValue; break;
                case 13: synthParamValue13 = newValue; break;
                case 14: synthParamValue14 = newValue; break;
                case 15: synthParamValue15 = newValue; break;
                case 16: synthParamValue16 = newValue; break;
                case 17: synthParamValue17 = newValue; break;
                case 18: synthParamValue18 = newValue; break;
                case 19: synthParamValue19 = newValue; break;
                case 20: synthParamValue20 = newValue; break;
                case 21: synthParamValue21 = newValue; break;
                case 22: synthParamValue22 = newValue; break;
                case 23: synthParamValue23 = newValue; break;   
                case 24: synthParamValue24 = newValue; break; 
                case 25: synthParamValue25 = newValue; break;    
                case 26: synthParamValue26 = newValue; break;
                case 27: synthParamValue27 = newValue; break;
                case 28: synthParamValue28 = newValue; break;
                case 29: synthParamValue29 = newValue; break;   
                case 30: synthParamValue30 = newValue; break; 
                case 31: synthParamValue31 = newValue; break;      
                case 32: synthParamValue32 = newValue; break;    
                case 33: synthParamValue33 = newValue; break;
                case 34: synthParamValue34 = newValue; break;
                case 35: synthParamValue35 = newValue; break;
                case 36: synthParamValue36 = newValue; break;   
                case 37: synthParamValue37 = newValue; break; 
                case 38: synthParamValue38 = newValue; break;  
                case 39: synthParamValue39 = newValue; break;    
                case 40: synthParamValue40 = newValue; break;
                case 41: synthParamValue41 = newValue; break;
                case 42: synthParamValue42 = newValue; break;
                case 43: synthParamValue43 = newValue; break;   
                case 44: synthParamValue44 = newValue; break; 
                case 45: synthParamValue45 = newValue; break; 
                case 46: synthParamValue41 = newValue; break;
                case 47: synthParamValue42 = newValue; break;
                case 48: synthParamValue43 = newValue; break;   
                case 49: synthParamValue44 = newValue; break; 
                case 50: synthParamValue45 = newValue; break;                                                                  
            }
        }

        /// <summary>
        /// Passes along all parameter values to native code.
        /// </summary>
        public void UpdateAllParameters()
        {
            float[] paramValues =
            {
                synthParamValue0, synthParamValue1, synthParamValue2, synthParamValue3,
                synthParamValue4, synthParamValue5, synthParamValue6, synthParamValue7,
                synthParamValue8, synthParamValue9, synthParamValue10, synthParamValue11,
                synthParamValue12, synthParamValue13, synthParamValue14, synthParamValue15,
                synthParamValue16, synthParamValue17, synthParamValue18, synthParamValue19,
                synthParamValue20, synthParamValue21, synthParamValue22, synthParamValue23,
                synthParamValue24, synthParamValue25, synthParamValue26, synthParamValue27, 
                synthParamValue28, synthParamValue29, synthParamValue30, synthParamValue31,
                synthParamValue32, synthParamValue33, synthParamValue34, synthParamValue35,
                synthParamValue36, synthParamValue37, synthParamValue38, synthParamValue39, 
                synthParamValue40, synthParamValue41, synthParamValue41, synthParamValue43, 
                synthParamValue44, synthParamValue45, synthParamValue46, synthParamValue47, 
                synthParamValue48, synthParamValue49, synthParamValue50                                   
            };
            for (int i = 0; i < synthParameters.Count; ++i)
                synthParameters[i].paramValue = paramValues[i];
        }

        /// <summary>
        /// Passes along parameter value to native code.
        /// </summary>
        /// <param name="index">The index of the parameter to update.</param>
        public void UpdateParameter(int index)
        {
            if (index >= synthParameters.Count || index < 0)
                return;
            float[] paramValues =
            {
                synthParamValue0, synthParamValue1, synthParamValue2, synthParamValue3,
                synthParamValue4, synthParamValue5, synthParamValue6, synthParamValue7,
                synthParamValue8, synthParamValue9, synthParamValue10, synthParamValue11,
                synthParamValue12, synthParamValue13, synthParamValue14, synthParamValue15,
                synthParamValue16, synthParamValue17, synthParamValue18, synthParamValue19,
                synthParamValue20, synthParamValue21, synthParamValue22, synthParamValue23,
                synthParamValue24, synthParamValue25, synthParamValue26, synthParamValue27, 
                synthParamValue28, synthParamValue29, synthParamValue30, synthParamValue31,
                synthParamValue32, synthParamValue33, synthParamValue34, synthParamValue35,
                synthParamValue36, synthParamValue37, synthParamValue38, synthParamValue39, 
                synthParamValue40, synthParamValue41, synthParamValue42, synthParamValue43,
                synthParamValue44, synthParamValue45, synthParamValue46, synthParamValue47, 
                synthParamValue48, synthParamValue49, synthParamValue50                                
            };
            synthParameters[index].paramValue = paramValues[index];
        }

        /// <summary>
        /// Adds an empty parameter placement.
        /// </summary>
        /// <returns>The create synth parameter object.</returns>
        public HelmParameter AddEmptyParameter()
        {
            if (synthParameters.Count >= MAX_PARAMETERS)
                return null;
            HelmParameter synthParameter = new HelmParameter(this);
            synthParameters.Add(synthParameter);
            return synthParameter;
        }

        /// <summary>
        /// Adds a new parameter to this controller.
        /// </summary>
        /// <returns>The create synth parameter object.</returns>
        public HelmParameter AddParameter(Param parameter)
        {
            HelmParameter synthParameter = new HelmParameter(this, parameter);
            SetParameterAtIndex(synthParameters.Count, synthParameter.paramValue);
            synthParameters.Add(synthParameter);
            return synthParameter;
        }

        /// <summary>
        /// Removes the given parameter from the parameter control list.
        /// </summary>
        /// <returns>The index where the parameter was.</returns>
        public int RemoveParameter(HelmParameter parameter)
        {
            int indexOf = synthParameters.IndexOf(parameter);
            synthParameters.Remove(parameter);
            return indexOf;
        }

        /// <summary>
        /// Gets a Helm synthesizer parameter's percent value of the valid range.
        /// e.g. When getting a transpose value, getting the value returns the number of semitones.
        /// </summary>
        /// <param name="parameter">The parameter to get the value of.</param>
        /// <returns>The current value of the parameter passed in.</returns>
        public float GetParameterValue(Param parameter)
        {
            return Native.HelmGetParameterValue(channel, (int)parameter);
        }

        /// <summary>
        /// Changes a Helm synthesizer parameter value.
        /// e.g. Lower the pitch of the oscillator by setting the transpose to -12 semitones.
        /// </summary>
        /// <param name="parameter">The parameter to be changed.</param>
        /// <param name="newValue">The value to change the parameter to.</param>
        public void SetParameterValue(Param parameter, float newValue)
        {
            Native.HelmSetParameterValue(channel, (int)parameter, newValue);
        }

        /// <summary>
        /// Gets a Helm synthesizer parameter's percent value of the valid range.
        /// e.g. When getting a transpose value, getting the value returns the number of semitones.
        /// </summary>
        /// <param name="parameter">The parameter to get the value of.</param>
        /// <returns>The current value of the parameter passed in.</returns>
        public float GetParameterValue(CommonParam parameter)
        {
            return Native.HelmGetParameterPercent(channel, (int)parameter);
        }

        /// <summary>
        /// Changes a Helm synthesizer parameter value.
        /// e.g. Lower the pitch of the oscillator by setting the transpose to -12 semitones.
        /// </summary>
        /// <param name="parameter">The parameter to be changed.</param>
        /// <param name="newValue">The value to change the parameter to.</param>
        public void SetParameterValue(CommonParam parameter, float newValue)
        {
            Native.HelmSetParameterValue(channel, (int)parameter, newValue);
        }

        /// <summary>
        /// Gets a Helm synthesizer parameter's percent value of the valid range.
        /// e.g. If the percent of a parameter is set to 1.0, it's at its max setting.
        /// </summary>
        /// <param name="parameter">The parameter to get the value percent of.</param>
        /// <returns>The current percent value of the parameter passed in.</returns>
        public float GetParameterPercent(Param parameter)
        {
            return Native.HelmGetParameterPercent(channel, (int)parameter);
        }

        /// <summary>
        /// Changes a Helm synthesizer parameter by percent of the valid range.
        /// e.g. Set the volume to the highest setting by setting the Volume to 1.0.
        /// </summary>
        /// <param name="parameter">The parameter to be changed.</param>
        /// <param name="newPercent">The percent of range of values to use. 0.0 for lowest, 1.0 for highest.</param>
        public void SetParameterPercent(Param parameter, float newPercent)
        {
            Native.HelmSetParameterPercent(channel, (int)parameter, newPercent);
        }

        /// <summary>
        /// Gets a Helm synthesizer parameter's percent value of the valid range.
        /// e.g. If the percent of a parameter is set to 1.0, it's at its max setting.
        /// </summary>
        /// <param name="parameter">The parameter to get the value percent of.</param>
        /// <returns>The current percent value of the parameter passed in.</returns>
        public float GetParameterPercent(CommonParam parameter)
        {
            return Native.HelmGetParameterPercent(channel, (int)parameter);
        }

        /// <summary>
        /// Changes a Helm synthesizer parameter by percent of the valid range.
        /// e.g. Set any parameter to the highest setting by setting the percent to 1.0.
        /// </summary>
        /// <param name="parameter">The parameter to be changed.</param>
        /// <param name="newPercent">The percent of range of values to use. 0.0 for lowest, 1.0 for highest.</param>
        public void SetParameterPercent(CommonParam parameter, float newPercent)
        {
            Native.HelmSetParameterPercent(channel, (int)parameter, newPercent);
        }

        /// <summary>
        /// Sets the polyphony (number of voices) of the synthesizer.
        /// Lower the polyphony to keep the DSP usage down.
        /// </summary>
        /// <param name="numVoices">The </param>
        public void SetPolyphony(int numVoices)
        {
            SetParameterValue(Param.kPolyphony, numVoices);
        }

        /// <summary>
        /// Triggers note off events for all notes currently on in the referenced Helm instance(s).
        /// </summary>
        public void AllNotesOff()
        {
            Native.HelmAllNotesOff(channel);
            pressedNotes.Clear();
            pressedFrequencies.Clear();
        }

        /// <summary>
        /// Checks if a note is currently on.
        /// </summary>
        /// <returns><c>true</c>, if note is currently on (held down), <c>false</c> otherwise.</returns>
        /// <param name="note">Note.</param>
        public bool IsNoteOn(int note)
        {
            return pressedNotes.ContainsKey(note);
        }

        /// <summary>
        /// Checks if a frequency is currently on.
        /// </summary>
        /// <returns><c>true</c>, if the frequency is currently on, <c>false</c> otherwise.</returns>
        /// <param name="frequency">Frequency in hertz.</param>
        public bool IsFrequencyOn(float frequency)
        {
            return pressedFrequencies.ContainsKey(frequency);
        }

        /// <summary>
        /// Gets a Dictionary of all the currently pressed notes.
        /// </summary>
        /// <returns>The pressed notes where the key is the MIDI number and the value is the number of active note on events.</returns>
        public Dictionary<int, int> GetPressedNotes()
        {
            return pressedNotes;
        }

        /// <summary>
        /// Triggers a note-on event for the Helm instance(s) this points to.
        /// After length amount of seconds, will automatically trigger a note off event.
        /// </summary>
        /// <param name="note">The MIDI keyboard note to play. [0, 127]</param>
        /// <param name="velocity">How hard you hit the key. [0.0, 1.0]</param>
        /// <param name="length">The time in seconds the note should play for.</param>
        public void NoteOn(int note, float velocity, float length)
        {
            NoteOn(note, velocity);
            StartCoroutine(WaitNoteOff(note, length));
        }

        /// <summary>
        /// Triggers a note on event for the Helm instance(s) this points to.
        /// You must trigger a note off event later for this note by calling NoteOff.
        /// </summary>
        /// <param name="note">The MIDI keyboard note to play. [0, 127]</param>
        /// <param name="velocity">How hard you hit the key. [0.0, 1.0]</param>
        public void NoteOn(int note, float velocity = 1.0f)
        {
            int number = 0;
            pressedNotes.TryGetValue(note, out number);
            pressedNotes[note] = number + 1;
            Native.HelmNoteOn(channel, note, velocity);
        }

        IEnumerator WaitNoteOff(int note, float length)
        {
            yield return new WaitForSeconds(length);
            NoteOff(note);
        }

        /// <summary>
        /// Triggers a note off event for the Helm instance(s) this points to.
        /// </summary>
        /// <param name="note">The MIDI keyboard note to turn off. [0, 127]</param>
        public void NoteOff(int note)
        {
            int number = 0;
            pressedNotes.TryGetValue(note, out number);
            if (number <= 1)
            {
                pressedNotes.Remove(note);
                Native.HelmNoteOff(channel, note);
            }
            else
                pressedNotes[note] = number - 1;
        }

        /// <summary>
        /// Triggers a note-on event for the Helm instance(s) this points to.
        /// Instead of a midi note, uses a frequency measured in hertz.
        /// After length amount of seconds, will automatically trigger a note off event.
        /// </summary>
        /// <param name="frequency">The frequency in hertz to play.</param>
        /// <param name="velocity">How hard you hit the key. [0.0, 1.0]</param>
        /// <param name="length">The time in seconds the note should play for.</param>
        public void FrequencyOn(float frequency, float velocity, float length)
        {
            FrequencyOn(frequency, velocity);
            StartCoroutine(WaitFrequencyOff(frequency, length));
        }

        /// <summary>
        /// Triggers a note on event for the Helm instance(s) this points to.
        /// Instead of a midi note, uses a frequency measured in hertz.
        /// You must trigger a note off event later for this note by calling NoteOff.
        /// </summary>
        /// <param name="frequency">The frequency in hertz to play.</param>
        /// <param name="velocity">How hard you hit the key. [0.0, 1.0]</param>
        public void FrequencyOn(float frequency, float velocity = 1.0f)
        {
            int number = 0;
            pressedFrequencies.TryGetValue(frequency, out number);
            pressedFrequencies[frequency] = number + 1;
            Native.HelmFrequencyOn(channel, frequency, velocity);
        }

        IEnumerator WaitFrequencyOff(float frequency, float length)
        {
            yield return new WaitForSeconds(length);
            FrequencyOff(frequency);
        }

        /// <summary>
        /// Triggers a note off event for the Helm instance(s) this points to.
        /// Instead of a midi note, uses a frequency measured in hertz.
        /// </summary>
        /// <param name="frequency">The frequency measured in hertz to turn off.</param>
        public void FrequencyOff(float frequency)
        {
            int number = 0;
            pressedFrequencies.TryGetValue(frequency, out number);
            if (number <= 1)
            {
                pressedFrequencies.Remove(frequency);
                Native.HelmFrequencyOff(channel, frequency);
            }
            else
                pressedFrequencies[frequency] = number - 1;
        }

        /// <summary>
        /// Sets the pitch wheel value for the synth. The pitch wheel bends the pitch of the synthesizer up or down.
        /// </summary>
        /// <param name="wheelValue">The new wheel value. [-1.0, 1.0]</param>
        public void SetPitchWheel(float wheelValue)
        {
            Native.HelmSetPitchWheel(channel, wheelValue);
        }


        /// <summary>
        /// Sets the modulation wheel value for the synth. The modulation wheel may change how the synth sounds depending on the patch.
        /// </summary>
        /// <param name="wheelValue">The new wheel value. [0.0, 1.0]</param>
        public void SetModWheel(float wheelValue)
        {
            Native.HelmSetModWheel(channel, wheelValue);
        }

		/// <summary>
		/// Sets the aftertouch for a given note. The aftertouch may change how the given note sounds depending on the patch.
		/// </summary>
		/// <param name="note">The note to change the aftertouch value on.</param>
		/// <param name="aftertouchValue">The new aftertouch value.</param>
		public void SetAftertouch(int note, float aftertouchValue)
        {
            Native.HelmSetAftertouch(channel, note, aftertouchValue);
        }

        void FixedUpdate()
        {
            // We wait until synth is active to update parameters.
            if (Time.timeSinceLevelLoad > UPDATE_WAIT)
                UpdateAllParameters();
        }
    }
}
