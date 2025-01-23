using System.Collections.Generic;
using System.IO;
using Sanford.Multimedia.Midi;
using kmusic.kmusicMIDI;

public class MidiExporter
{
    public void ExportMidiWithSanford(
        string filename,
        float bpm,
        List<kMidiNote> helmNotes,
        List<kMidiNote> sampleNotes,
        List<kMidiNote> drumNotes)
    {
        var sequence = new Sequence();

        // Add Helm Pattern to Track 1
        var helmTrack = new Track();
        sequence.Add(helmTrack);
        AddTempoEvent(helmTrack, bpm);
        AddNotesToTrack(helmTrack, helmNotes);

        // Add Sample Pattern to Track 2
        var sampleTrack = new Track();
        sequence.Add(sampleTrack);
        AddTempoEvent(sampleTrack, bpm);
        AddNotesToTrack(sampleTrack, sampleNotes);

        // Add Drum Pattern to Track 3
        var drumTrack = new Track();
        sequence.Add(drumTrack);
        AddTempoEvent(drumTrack, bpm);
        AddNotesToTrack(drumTrack, drumNotes);

        // Save MIDI file
        string path = Path.Combine(UnityEngine.Application.persistentDataPath, filename + ".mid");
        sequence.Save(path);

        UnityEngine.Debug.Log($"MIDI file saved at: {path}");
    }

    private void AddTempoEvent(Track track, float bpm)
    {
        int tempo = 60000000 / (int)bpm;
        var tempoBytes = new byte[]
        {
            (byte)(tempo >> 16),
            (byte)(tempo >> 8),
            (byte)tempo
        };

        var tempoEvent = new MetaMessage(MetaType.Tempo, tempoBytes);
        track.Insert(0, tempoEvent); // Add tempo at the beginning
    }

    private void AddNotesToTrack(Track track, List<kMidiNote> notes)
    {
        int ticksPer16thNote = 120; // Adjust this value as needed
        foreach (var note in notes)
        {
            int startTick = (int)(note.Start * ticksPer16thNote);
            int durationTick = (int)((note.End - note.Start) * ticksPer16thNote);

            // NoteOn event
            var noteOn = new ChannelMessage(ChannelCommand.NoteOn, 0, note.Note, (int)(note.Velocity * 127));
            track.Insert(startTick, noteOn);

            // NoteOff event
            var noteOff = new ChannelMessage(ChannelCommand.NoteOff, 0, note.Note, 0);
            track.Insert(startTick + durationTick, noteOff);
        }
    }
}
