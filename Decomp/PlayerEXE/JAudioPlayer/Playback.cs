using JAudio;
using JAudio.Sequence;
using JAudio.SoundData;
using Multimedia;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAPO.Fx;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JAudioPlayer
{
	internal class Playback
	{
		private class TrackData
		{
			public int NextEventTime
			{
				get;
				set;
			}

			public int EventIndex
			{
				get;
				set;
			}
		}

		private class Track
		{
			private class NoteData
			{
				public enum VoiceState
				{
					Active,
					Attack,
					Release,
					Disposed
				}

				public SourceVoice Voice
				{
					get;
					private set;
				}

				public DataStream BufferStream
				{
					get;
					set;
				}

				public float Velocity
				{
					get;
					private set;
				}

				public byte PercussionPan
				{
					get;
					set;
				}

				public float FrequencyRatio
				{
					get;
					private set;
				}

				public bool IsPercussion
				{
					get;
					private set;
				}

				public float FadeVolume
				{
					get;
					set;
				}

				public VoiceState State
				{
					get;
					set;
				}

				public NoteData(SourceVoice v, float freqRatio, byte velocity = 127, bool perc = false)
				{
					if (velocity > 127)
					{
						throw new ArgumentOutOfRangeException("velocity", "Velocity cannot be greater than 127.");
					}
					Voice = v;
					FrequencyRatio = freqRatio;
					Velocity = 0f;
					IsPercussion = perc;
					PercussionPan = 63;
					FadeVolume = 1f;
					State = VoiceState.Active;
				}
			}

			private float[] outputMatrix;

			private byte _Pan;

			private bool panChanged;

			private short _Pitch;

			private byte _Echo;

			private bool echoChanged;

			private readonly XAudio2 engine;

			private readonly SubmixVoice submix;

			private readonly Z2Sound soundData;

			private readonly Dictionary<Tuple<byte, byte>, Instrument> instrumentTable;

			private readonly Dictionary<int, Sample> sampleTable;

			private Dictionary<byte, NoteData> voiceTable;

			public byte Bank
			{
				get;
				set;
			}

			public byte Instrument
			{
				get;
				set;
			}

			public byte Volume
			{
				get;
				set;
			}

			public byte Pan
			{
				get
				{
					return _Pan;
				}
				set
				{
					_Pan = value;
					sbyte b = (sbyte)(_Pan - 63);
					outputMatrix[0] = 0.5f - (float)b / 128f;
					outputMatrix[1] = 0.5f + (float)b / 128f;
					panChanged = true;
				}
			}

			public byte Echo
			{
				get
				{
					return _Echo;
				}
				set
				{
					_Echo = value;
					echoChanged = true;
				}
			}

			public short Pitch
			{
				get
				{
					return _Pitch;
				}
				set
				{
					_Pitch = value;
					double num = Convert.ToDouble(value);
					if (num > Math.Pow(2.0, 15.0))
					{
						num -= Math.Pow(2.0, 16.0);
					}
					double num2 = num / 682.66666666666663;
					PitchFrequencyRatio = XAudio2.SemitonesToFrequencyRatio((float)num2);
				}
			}

			private float PitchFrequencyRatio
			{
				get;
				set;
			}

			public Track(XAudio2 xAudio2, Z2Sound baa, Dictionary<Tuple<byte, byte>, Instrument> instrumentTable, Dictionary<int, Sample> sampleTable)
			{
				engine = xAudio2;
				soundData = baa;
				this.instrumentTable = instrumentTable;
				this.sampleTable = sampleTable;
				voiceTable = new Dictionary<byte, NoteData>();
				submix = new SubmixVoice(engine);
				Echo effect = new Echo();
				EffectDescriptor effectDescriptor = new EffectDescriptor(effect)
				{
					InitialState = true,
					OutputChannelCount = 2
				};
				submix.SetEffectChain(effectDescriptor);
				EchoParameters effectParameter = new EchoParameters
				{
					Delay = 160f,
					Feedback = 0f,
					WetDryMix = 0f
				};
				submix.SetEffectParameters(0, effectParameter);
				submix.EnableEffect(0);
				outputMatrix = new float[2];
				Volume = 127;
				Pitch = 0;
				Pan = 63;
				panChanged = false;
				Bank = 0;
				Echo = 0;
				echoChanged = false;
				Instrument = 0;
				outputMatrix[0] = (outputMatrix[1] = 0.5f);
			}

			public void Update(int deltaMs)
			{
				foreach (KeyValuePair<byte, NoteData> item in voiceTable)
				{
					item.Value.Voice.SetFrequencyRatio(item.Value.FrequencyRatio * PitchFrequencyRatio);
					if (panChanged && !item.Value.IsPercussion)
					{
						item.Value.Voice.SetOutputMatrix(1, 2, outputMatrix);
					}
				}
				submix.SetVolume(ValueToAmplitude(Volume));
				if (panChanged)
				{
					panChanged = false;
				}
				if (echoChanged)
				{
					EchoParameters effectParameter = default(EchoParameters);
					effectParameter.Delay = 160f;
					effectParameter.Feedback = 0.6f;
					effectParameter.WetDryMix = (float)(int)Echo / 384f;
					submix.SetEffectParameters(0, effectParameter);
					echoChanged = false;
				}
			}

			public void NoteOn(byte num, byte key, byte velocity)
			{
				try
				{
					Instrument instrument = instrumentTable[new Tuple<byte, byte>(Bank, Instrument)];
					int value = instrument.GetSampleEntry(key).Value;
					Sample sample = soundData.SampleBanks[soundData.InstrumentBanks[Bank].Wsys][(short)instrument.Samples[value].Id];
					AudioBuffer audioBuffer = new AudioBuffer();
					audioBuffer.AudioBytes = sample.Data.Length;
					audioBuffer.Stream = new DataStream(sample.Data.Length, canRead: true, canWrite: true);
					audioBuffer.Stream.Write(sample.Data, 0, sample.Data.Length);
					audioBuffer.PlayBegin = 0;
					audioBuffer.PlayLength = 0;
					audioBuffer.LoopCount = (sample.IsLooping ? 255 : 0);
					audioBuffer.LoopBegin = sample.LoopStart;
					audioBuffer.LoopLength = 0;
					WaveFormat sourceFormat = new WaveFormat(sample.SamplesPerSecond, sample.BitsPerSample, sample.Channels);
					SourceVoice sourceVoice = new SourceVoice(engine, sourceFormat, VoiceFlags.None, 1024f);
					sourceVoice.SetOutputVoices(new VoiceSendDescriptor(submix));
					sourceVoice.SubmitSourceBuffer(audioBuffer, null);
					float num2 = XAudio2.SemitonesToFrequencyRatio((!instrument.IsPercussion) ? (key - sample.RootKey) : 0) * instrument.Samples[value].FrequenyMultiplier * instrument.FrequencyMultiplier;
					sourceVoice.SetFrequencyRatio(num2);
					sourceVoice.SetVolume(ValueToAmplitude(velocity));
					NoteData noteData = new NoteData(sourceVoice, num2, velocity, instrument.IsPercussion);
					if (instrument.IsPercussion)
					{
						noteData.PercussionPan = instrument.Samples[value].Pan.Value;
						sbyte b = (sbyte)(noteData.PercussionPan - 63);
						sourceVoice.SetOutputMatrix(1, 2, new float[2]
						{
							0.5f - (float)b / 127f,
							0.5f + (float)b / 127f
						});
					}
					else
					{
						sourceVoice.SetOutputMatrix(1, 2, outputMatrix);
					}
					noteData.BufferStream = audioBuffer.Stream;
					noteData.Voice.Start();
					voiceTable.Add(num, noteData);
					sourceFormat = null;
					GC.Collect();
				}
				catch
				{
				}
			}

			public void NoteOff(byte num)
			{
				if (voiceTable.ContainsKey(num))
				{
					voiceTable[num].Voice.Stop();
					voiceTable[num].Voice.DestroyVoice();
					voiceTable[num].Voice.Dispose();
					voiceTable[num].BufferStream.Dispose();
					voiceTable[num] = null;
					voiceTable.Remove(num);
				}
			}

			public void AllNotesOff()
			{
				foreach (byte item in voiceTable.Keys.ToList())
				{
					NoteOff(item);
				}
			}

			private static float ValueToAmplitude(byte value)
			{
				if (value > 127)
				{
					throw new ArgumentOutOfRangeException("value", "The velocity cannot be greater than 127.");
				}
				return (float)Math.Pow((float)(int)value / 127f, 2.0);
			}
		}

		private Z2Sound soundData;

		private MasteringVoice masteringVoice;

		private XAudio2 musicEngine;

		private Timer timer;

		private Bms sequence;

		private List<TrackData> trackData;

		private List<Track> tracks;

		private Dictionary<Tuple<byte, byte>, Instrument> instrumentTable;

		private Dictionary<int, Sample> sampleTable;

		public bool IsPlaying
		{
			get;
			set;
		}

		public Bms Sequence
		{
			get
			{
				return sequence;
			}
			set
			{
				if (IsPlaying)
				{
					throw new InvalidOperationException("The sequence cannot be set during playback.");
				}
				sequence = value;
				Time = 0;
				FractionalTicks = 0;
				TicksPerBeat = 120;
				Tempo = 120;
				Preload();
			}
		}

		private int Time
		{
			get;
			set;
		}

		private int FractionalTicks
		{
			get;
			set;
		}

		private int TicksPerBeat
		{
			get;
			set;
		}

		private int Tempo
		{
			get;
			set;
		}

		public Playback(string soundPath)
		{
			Z2Sound.SoundPath = soundPath;
			soundData = new Z2Sound(File.OpenRead(soundPath + "\\Z2Sound.baa"));
			musicEngine = new XAudio2();
			masteringVoice = new MasteringVoice(musicEngine);
			timer = new Timer();
			timer.Period = 1;
			timer.Resolution = 1;
			timer.Tick += HandleTick;
			IsPlaying = false;
		}

		~Playback()
		{
			if (IsPlaying)
			{
				Stop();
			}
		}

		public void Start()
		{
			if (Sequence == null)
			{
				throw new InvalidOperationException("A sequence was not specified.");
			}
			IsPlaying = true;
			timer.Start();
		}

		public void Stop()
		{
			timer.Stop();
			foreach (Track track in tracks)
			{
				track.AllNotesOff();
			}
			GC.Collect();
			IsPlaying = false;
			Time = 0;
			FractionalTicks = 0;
		}

		private void Preload()
		{
			List<Tuple<byte, byte>> list = new List<Tuple<byte, byte>>();
			trackData = new List<TrackData>();
			trackData.Add(new TrackData());
			tracks = new List<Track>(Sequence.Tracks.Count);
			instrumentTable = new Dictionary<Tuple<byte, byte>, Instrument>();
			sampleTable = new Dictionary<int, Sample>();
			foreach (JAudio.Sequence.Track track in Sequence.Tracks)
			{
				trackData.Add(new TrackData());
				tracks.Add(new Track(musicEngine, soundData, instrumentTable, sampleTable));
				byte item = 0;
				foreach (Event datum in track.Data)
				{
					if (datum is BankSelectEvent)
					{
						item = ((BankSelectEvent)datum).Bank;
					}
					else if (datum is InstrumentChangeEvent)
					{
						list.Add(new Tuple<byte, byte>(item, ((InstrumentChangeEvent)datum).Instrument));
					}
				}
			}
			list = list.Distinct().ToList();
			foreach (Tuple<byte, byte> item2 in list)
			{
				if (item2.Item1 != 0)
				{
					Instrument instrument = soundData.InstrumentBanks[item2.Item1][item2.Item2];
					instrumentTable.Add(item2, instrument);
					foreach (SampleEntry sample in instrument.Samples)
					{
						try
						{
							if (!sampleTable.ContainsKey(sample.Id))
							{
								sampleTable.Add(sample.Id, soundData.SampleBanks[soundData.InstrumentBanks[item2.Item1].Wsys][(short)sample.Id]);
							}
						}
						catch (SampleNotFoundException)
						{
						}
					}
				}
			}
		}

		private void HandleTick(object sender, EventArgs e)
		{
			int num = GenerateTicks();
			for (int i = 0; i < num; i++)
			{
				OnTick();
			}
		}

		private void OnTick()
		{
			Time++;
			if (Time - 1 == trackData[0].NextEventTime)
			{
				int num = trackData[0].EventIndex - 1;
				while (num < Sequence.MetaTrack.Count - 1)
				{
					num++;
					Event @event = Sequence.MetaTrack[num];
					if (@event is DelayEvent)
					{
						trackData[0].NextEventTime += (@event as DelayEvent).Delay;
						break;
					}
					if (@event is TerminateEvent)
					{
						return;
					}
					if (@event is TimeResolutionEvent)
					{
						TicksPerBeat = (@event as TimeResolutionEvent).TimeResolution;
					}
					else if (@event is TempoEvent)
					{
						Tempo = ((TempoEvent)@event).Tempo;
					}
				}
				trackData[0].EventIndex = num + 1;
			}
			for (int i = 0; i < Sequence.Tracks.Count; i++)
			{
				if (Time - 1 == trackData[i + 1].NextEventTime)
				{
					int num2 = trackData[i + 1].EventIndex - 1;
					while (num2 < Sequence.Tracks[i].Data.Count - 1)
					{
						num2++;
						Event event2 = Sequence.Tracks[i].Data[num2];
						if (event2 is DelayEvent)
						{
							trackData[i + 1].NextEventTime += (event2 as DelayEvent).Delay;
							break;
						}
						if (event2 is BankSelectEvent)
						{
							tracks[i].Bank = (event2 as BankSelectEvent).Bank;
						}
						else if (event2 is InstrumentChangeEvent)
						{
							tracks[i].Instrument = (event2 as InstrumentChangeEvent).Instrument;
						}
						else if (event2 is PanEvent)
						{
							tracks[i].Pan = (event2 as PanEvent).Pan;
						}
						else if (event2 is NoteOnEvent)
						{
							tracks[i].NoteOn((event2 as NoteOnEvent).Number, (event2 as NoteOnEvent).Note, (event2 as NoteOnEvent).Velocity);
						}
						else if (event2 is NoteOffEvent)
						{
							tracks[i].NoteOff((event2 as NoteOffEvent).Number);
						}
						else if (event2 is VolumeEvent)
						{
							tracks[i].Volume = (event2 as VolumeEvent).Volume;
						}
						else if (event2 is ReverbEvent)
						{
							tracks[i].Echo = (event2 as ReverbEvent).Reverb;
						}
						else if (event2 is PitchEvent)
						{
							tracks[i].Pitch = (event2 as PitchEvent).Pitch;
						}
					}
					trackData[i + 1].EventIndex = num2 + 1;
				}
				tracks[i].Update((int)Math.Round(60000.0 / (double)TicksPerBeat / (double)Tempo));
			}
		}

		private int GenerateTicks()
		{
			int num = 1000 * TicksPerBeat * timer.Period;
			int num2 = 60000000 / Tempo;
			int num3 = (FractionalTicks + num) / num2;
			FractionalTicks += num - num3 * num2;
			return num3;
		}
	}
}
