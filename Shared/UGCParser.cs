using System;
using CitizenFX.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Linq;
using CitizenFX.Core.Native;
using System.Dynamic;
using System.IO;
using System.Net.NetworkInformation;

namespace racing
{
	public static partial class UGC
	{

		/// <summary>
		/// This empty <see cref="JArray"/> is used as a placeholder for missing data when reading from UGC
		/// </summary>
		internal static JArray missingData = new JArray();

		internal static bool ContainsKeys(this JObject o, params string[] keys)
		{
			return keys.All(key => o.ContainsKey(key));
		}

		internal static object TryGet(this JObject o, string key, object defaultValue)
		{
			return o.ContainsKey(key) ? o[key] : defaultValue;
		}

		internal static JArray TryGetArray(this JObject o, string key, JArray defaultValue = null)
		{
			if (o.ContainsKey(key))
			{
				var potentialArray = o[key];
				if (potentialArray.Type == JTokenType.Array)
					return (JArray)potentialArray;
			}

			return defaultValue ?? null;
		}

		public static Vector3 ToVector3(this JToken t)
		{
			if (t.Type == JTokenType.Object)
			{
				var o = (JObject)t;
				if (o.HasValues && o.ContainsKeys("x", "y", "z"))
					return new Vector3((float)o["x"], (float)o["y"], (float)o["z"]);
			}

			return Vector3.Zero;
		}

		[Serializable]
		public struct CheckpointDefinition
		{
			public Vector3 Location;
			public float Heading;
			public float Scale;
			public bool IsRound;
			//public int WrongWayTimer;
			//public CheckpointDefinition SecondCheckpoint;


			/* // All values relating to checkpoints
			Var10 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chh");		// Heading
			iVar11 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chs");		// Scale
			iVar12 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chs2");		// Scale (2)
			iVar13 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chvs");		//
			iVar14 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chpp");		//
			iVar15 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chpps");		//
			iVar16 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chl");		// Location
			iVar17 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "sndchk");	// Location (2)
			iVar18 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "sndrsp");	// 
			iVar19 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpwwt");		// Wrong Way Time
			iVar20 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cppsst");	
			iVar21 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpado");
			iVar22 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpados");
			iVar23 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chttu");
			iVar24 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chttr");
			iVar25 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpbs1");		
			iVar26 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpbs2");
			iVar27 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cptfrm");
			iVar28 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cptfrms");
			iVar29 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "trfmvm");
			iVar30 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chdlo");
			iVar31 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chsto");
			iVar32 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chdlos");
			iVar33 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chstos");
			iVar34 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "rsp");
			iVar35 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cdsblcu");
			iVar36 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpdss");
			iVar37 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "rndchk");	// Is Round
			iVar38 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "rndchks");	// Is Round (2)
			iVar39 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpwtr");
			iVar40 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpwtrs");
			iVar41 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpair");		// Has second checkpoint
			*/

			/* // Old Constructors
			public CheckpointDefinition(Vector3 l, float h, float s, bool iR)
			{
				Location = l;
				Heading = h;
				Scale = s;
				IsRound = iR;
			}

			
			public CheckpointDefinition(dynamic obj)
			{
				try
				{
					Location = obj.Location;
					Heading = obj.Heading;
					Scale = obj.Scale;
					IsRound = obj.IsRound;
				} catch(Exception e)
				{
					throw new ArgumentException("Given object did not contain the data necessary to construct a CheckpointDefinition", "obj", e);
				}
			}*/
		}

		[Serializable]
		public struct PropDefinition
		{
			public float Heading;
			public Vector3 Location;
			public int Model;
			public Vector3 Rotation;
			public int EntityLODDist;
			public bool HasSpeedModifier;
			public int SpeedAmount;
			public float SpeedDuration;
			public int TextureVariant;
		}

		public class Map
		{
			public JObject Data { get; internal set; }
			public string Json { get; private set; }
			public JObject Mission { get => (JObject)Data["mission"]; }
			public JObject Race { get => (JObject)Data["mission"]["race"]; }
			public JObject Prop { get => (JObject)Data["mission"]["prop"]; }
			//public List<CheckpointDefinition> Checkpoints { get; private set; }
			//public List<PropDefinition> Props { get; private set; }
			public List<T> GetList<T>(string selector)
			{
				return Data.SelectToken(selector)?.Select(o => o.ToObject<T>())?.ToList();
			}

			public JObject GetObject(string selector)
			{
				return (JObject)Data.SelectToken(selector, false);
			}

			public T Get<T>(string selector)
			{
				return Data.SelectToken(selector, false).ToObject<T>();
			}

			public JToken Get(string selector)
			{
				return Data.SelectToken(selector, false);
			}

			public Map(string rawData)
			{
				Json = rawData;
				Data = JObject.Parse(rawData);
				//Props = new List<PropDefinition>();
				//Checkpoints = new List<CheckpointDefinition>();
			}
		}
	}
}

/* shelved code, come back to it when you've got proper time.
public class Map
{
	public string Json { get; private set; }
	public dynamic Data { get; private set; }
	//public dynamic this[string key] { get => Data.[key]; }

	/*
	UGC pseudo-specification

		- All valid JSON
		- All arrays contain a single data type. Therefore, arrays are List<T>'s.
		- Arrays cannot contain objects or other arrays (other than Vector3 encoded as an object)


	public Map(string jsonString)
	{
		try
		{
			JsonTextReader reader = new JsonTextReader(new StringReader(jsonString));
			Stack<IEnumerable> objectStack = new Stack<IEnumerable>();
			string nextProperty = null;
			bool currentObjectIsVector3 = false;
			List<float> vectorComponents = new List<float>();

			// Make peeking at the stack safe if it's empty.
			IEnumerable PeekAtObjectStack()
			{
				if (objectStack.Count > 0)
					return objectStack.Peek();

				return null;
			}

			void AddTokenToParent<ObjectType>(ObjectType newObject)
			{
				// If we're actually a Vector3 then add us to the vectorComponents list instead of assigning us anywhere else.
				if (currentObjectIsVector3)
				{
					if (typeof(ObjectType) == typeof(float))
					{
						vectorComponents.Add((float)(object)newObject);
					}
					else
					{
						throw new InvalidDataException("While attempting to parse an established Vector3 array, a value of a non-float type was attempted to be added to the Vector3!\nThis indicates invalid UGC!");
					}

					return;
				}

				IEnumerable parentCollection = PeekAtObjectStack();

				if (parentCollection == null) // && newObject is ExpandoObject) // When are we going to have an orphaned object??
				{
					// This is our root object, assign it to "Data"
					Data = (ExpandoObject)(object)newObject;
				}
				else if (parentCollection is ExpandoObject && nextProperty != null)
				{
					// Our parent is a dictionary, use the next property and add us to the parent at that key.
					((IDictionary<string, object>)parentCollection)[nextProperty] = newObject;
					nextProperty = null;
				}
				else if (parentCollection is List<ObjectType> list)
				{
					// Our parent is a strongly typed list, simply just add ourselves!
					list.Add(newObject);
				}
				else if (parentCollection is List<object>)
				{
					// We have a non strongly typed list, lets fix that!
					objectStack.Pop(); // pop our parent off the stack
					var parentOfParent = PeekAtObjectStack(); // Get its parent

					// As arrays cannot contain other arrays, the parent of our parent must be an ExpandoObject. Throw an exception if not!
					if (!(parentOfParent is ExpandoObject))
						throw new ArgumentException("UGC Data was invalid!");

					// Find the parent of us in its parent and remove it
					KeyValuePair<string, object> outObject = ((IDictionary<string, object>)parentOfParent).FirstOrDefault(obj => obj.Value == parentCollection);
					string ourKey = outObject.Key;
					((IDictionary<string, object>)parentOfParent).Remove(ourKey);

					// Cast our parent to a strongly typed list
					List<ObjectType> parentCollectionAsList = (List<ObjectType>)parentCollection.Cast<ObjectType>();

					// Add ourselves to our parent
					parentCollectionAsList.Add(newObject);

					// Add our parent back to its parent
					((IDictionary<string, object>)parentOfParent).Add(ourKey, parentCollection);

					// Push our parent back onto the stack, phew
					objectStack.Push(parentCollectionAsList);
				}
			}

			while (reader.Read())
			{
				if (reader.Value == null)
				{
					// This is a control token, no value associated with it
					switch (reader.TokenType)
					{
						case JsonToken.StartObject:
							// If we're an object inside of an array, we could potentially be a Vector3.
							// This can only be fully determined after we know the rest of our object but as arrays only have one type inside of them,
							// We can check the type of our parent!

							// So, we need to who our parent is...
							IEnumerable parent = PeekAtObjectStack();

							if (parent is List<Vector3>)
							{
								// Bingo! Set the flag and get out without actually making any objects.
								currentObjectIsVector3 = true;
								break;
							}

							// Create a new dictionary
							var newObject = new ExpandoObject();

							// Add the new object to the parent
							AddTokenToParent(newObject);

							// Push to the stack
							objectStack.Push(newObject);
							break;

						case JsonToken.StartArray:
							// Create a new list (of the generic object type) to the
							var newArray = new List<object>();

							// Add the new list to the parent
							AddTokenToParent(newArray);

							// Push to the stack
							objectStack.Push(newArray);
							break;

						case JsonToken.EndObject:
							if (currentObjectIsVector3 && vectorComponents.Count == 3)
							{
								// Thanks to constraints, it's possible to know at the object creation token that we might be a Vector3.
								// If that is so, we can fast track a lot of the checking and just add ourselves.

								// Get the vector!
								Vector3? newVector3 = null;
								try
								{
									newVector3 = new Vector3(vectorComponents[0], vectorComponents[1], vectorComponents[2]);
								}
								catch
								{
									Debug.WriteLine("Error parsing Vector3!");
									break;
								}

								// Something went very wrong!
								if (newVector3 is null)
									throw new InvalidDataException("An object being added to a List<Vector3> did not populate vectorComponents correctly or another error occurred!");

								// Add ourselves!
								AddTokenToParent((Vector3)newVector3);

								// Clear the flag and component list.
								currentObjectIsVector3 = false;
								vectorComponents.Clear();
							}
							else
							{
								// Objects can actually be Vector3s in disguise so, we need to figure out if we're a normal object or a Vector3.
								// We cannot have objects within arrays so, if we are within an array, we're actually a Vector3!
								// If we're in an object, it becomes a bit harder to tell but not impossible.

								// First, we're going to pop off in case we're actually not within an array.
								dynamic potentialVector3 = (ExpandoObject)objectStack.Pop(); // We're casting directly to an ExpandoObject as... we have to be one :P

								IEnumerable parentCollection = PeekAtObjectStack();

								if (parentCollection == null)
									break; // We are the root object, 100% not a Vector3!

								// We only need to do proper xyz checks if our parent is an object, otherwise, we're in an array and must be a Vector3.
								if (parentCollection is ExpandoObject)
								{
									var selfAsDict = (IDictionary<string, object>)potentialVector3;
									if (!selfAsDict.ContainsKey("x") || !selfAsDict.ContainsKey("y") || !selfAsDict.ContainsKey("z"))
										break; // No, we're not a Vector3.
								}

								// Time to get our Vector3!
								Vector3? newVector3 = null;
								try
								{
									newVector3 = new Vector3((float)potentialVector3.x, (float)potentialVector3.y, (float)potentialVector3.z);
								}
								catch
								{
									Debug.WriteLine("Error parsing Vector3!");
									break;
								}

								// If we make it here and newVector3 is somehow null, we're not a Vector3.
								if (newVector3 is null)
									break;

								// Now we remove ourselves from our parent container...
								if (parentCollection is ExpandoObject)
								{
									((IDictionary<string, object>)parentCollection).Remove(;
								}
								else
								{
									((IList)parentCollection).Remove(potentialVector3);
								}

								// And finally, add ourselves as a regular value (to ensure strong typing of List<object>s!)
								AddTokenToParent((Vector3)newVector3);
							}

							break;

						case JsonToken.EndArray:
							// Pop the last object on the stack as we should no longer add to it.
							objectStack.Pop();
							break;

						default:
							Console.WriteLine("Unhandled control token: {0}", reader.TokenType);
							break;
					}
				}
				else
				{
					// This is an item token, something with a value
					switch (reader.TokenType)
					{
						case JsonToken.PropertyName:
							nextProperty = (string)reader.Value;
							break;

						case JsonToken.Float:
							AddTokenToParent((float)reader.Value);
							break;

						case JsonToken.Boolean:
							AddTokenToParent((bool)reader.Value);
							break;

						case JsonToken.String:
							AddTokenToParent((string)reader.Value);
							break;

						case JsonToken.Integer:
							if (reader.ValueType == typeof(long))
							{
								AddTokenToParent((long)reader.Value);
							}
							else // gotta be an inty boi then?
							{
								AddTokenToParent((int)reader.Value);
							}

							break;

						default:
							Console.WriteLine("Unhandled value token: {0}", reader.TokenType);
							break;
					}
				}
			}
		}
		catch (Exception e)
		{
			Debug.WriteLine($"UGC Parser Error\n{e}");
		}
	}
}


	/*
	public class UGCData<T>
	{
		internal T Data;

		public static implicit operator T(UGCData<T> ugcData)
		{
			return ugcData.Data;
		}

		public IUGCData this[string key]
		{
			get
			{
				UGCData<U> outData = null;

				if (data is Dictionary<string, UGCData>)
					((Dictionary<string, UGCData>)data).TryGetValue(key, out outData);

				return outData;
			}
		}
	}

	// Imagine, using templates, lmao
	public class UGCData
	{
		private object data;

		public UGCData this[string key]
		{
			get
			{
				UGCData outData = null;

				if (data is IDictionary)
					((Dictionary<string, UGCData>)data).TryGetValue(key, out outData);

				return outData;
			}
		}

		public UGCData this[int index]
		{
			get
			{
				UGCData outData = null;

				if (data is IList<UGCData>)
				{
					IList<UGCData> dataAsList = (IList<UGCData>)data;
					if (dataAsList.Count >= index + 1)
					{
						outData = dataAsList[index];
					}
				}

				return outData is default(UGCData) ? null : outData;
			}
		}

		public UGCData(bool inputData)
		{
			data = inputData;
		}

		public UGCData(int inputData)
		{
			data = inputData;
		}

		public UGCData(float inputData)
		{
			data = inputData;
		}

		public UGCData(string inputData)
		{
			data = inputData;
		}

		public UGCData(List<UGCData> inputData)
		{
			data = inputData;
		}

		public UGCData(Vector3 inputData)
		{
			data = inputData;
		}

		public UGCData(Dictionary<string, UGCData> inputData)
		{
			data = inputData;
		}

		public static implicit operator bool(UGCData ugcData)
		{
			if (ugcData.data is bool)
				return (bool)ugcData.data;

			return default;
		}

		public static implicit operator int(UGCData ugcData)
		{
			if (ugcData.data is int)
				return (int)ugcData.data;

			return default;
		}

		public static implicit operator float(UGCData ugcData)
		{
			if (ugcData.data is float)
				return (float)ugcData.data;

			return default;
		}

		public static implicit operator string(UGCData ugcData)
		{
			if (ugcData.data is string)
				return (string)ugcData.data;

			return default;
		}

		public static implicit operator Vector3(UGCData ugcData)
		{
			if (ugcData.data is Vector3)
				return (Vector3)ugcData.data;

			return default;
		}

		public static implicit operator List<object>(UGCData ugcData)
		{
			if (ugcData.data is List<object>)
				return (List<object>)ugcData.data;

			return default;
		}

		public static implicit operator Dictionary<string, object>(UGCData ugcData)
		{
			if (ugcData.data is Dictionary<string, object>)
				return (Dictionary<string, object>)ugcData.data;

			return default;
		}


		internal static explicit operator UGCData(List<UGCData> list)
		{
			return new UGCData(list);
		}

		internal static explicit operator UGCData(Dictionary<string, UGCData> dict)
		{
			return new UGCData(dict);
		}
	}*/

/*

	public class Map
	{
		public string Json { get; private set; }
		public Dictionary<string, object> Data { get; private set; }
		public dynamic this[string key] { get => Data[key]; }

		private Vector3 DictToVector3(Dictionary<string, object> dict)
		{
			try
			{
				return new Vector3((float)dict["x"], (float)dict["y"], (float)dict["z"]);
			}
			catch (Exception e)
			{
				Console.WriteLine("vector3 parse fail");
				return Vector3.Zero;
			}

		}

		public Map(string jsonString)
		{
			// none of this is safe :(
			try
			{
				JsonTextReader reader = new JsonTextReader(new StringReader(jsonString));
				Stack<IEnumerable> objectStack = new Stack<IEnumerable>();
				string nextProperty = null;
				string vector3Tracker = null;

				IEnumerable PeekAtObjectStack() {
					if (objectStack.Count > 0)
						return objectStack.Peek();

					return null;
				}

				void AddValueToParent(object newObject)
				{
					IEnumerable parentCollection = PeekAtObjectStack() ?? null;

					if (parentCollection == null && newObject is Dictionary<string, object>)
					{
						// This is our root object, assign it to "Data"
						Data = (Dictionary<string, object>)newObject;
					}
					else if (parentCollection is IDictionary || nextProperty != null)
					{
						((IDictionary)parentCollection)[nextProperty] = newObject;
						nextProperty = null;
					}
					else if (parentCollection is IList)
					{
						((IList)parentCollection).Add(newObject);
					}
				}

				while (reader.Read())
				{
					if (reader.Value == null)
					{
						// This is a control token, no value associated with it
						switch (reader.TokenType)
						{
							case JsonToken.StartObject:
								// Create a new dictionary
								var newObject = new Dictionary<string, object>();

								// Add the new object to the parent
								AddValueToParent(newObject);

								// Push to the stack
								objectStack.Push(newObject);

								// Set the Vector3 constructor flag
								vector3Tracker = "x";
								break;

							case JsonToken.StartArray:
								// Create a new dictionary
								var newArray = new List<object>();

								// Add the new object to the parent
								AddValueToParent(newArray);

								// Push to the stack
								objectStack.Push(newArray);
								break;

							case JsonToken.EndObject:
								var potentialVector3Dict = objectStack.Pop();

								// Check if the object we're just about to end is actually a Vector3, if not, we've already popped off so ¯\_(ツ)_/¯
								if (vector3Tracker == "yes" && potentialVector3Dict is Dictionary<string, object>)
								{
									// Take the now completed dictionary off of the stack and turn it into a vector
									Vector3 newVector = DictToVector3((Dictionary<string, object>)potentialVector3Dict);

									// Get whatever the parent was, remove the dictionary from our parent and put the new Vector3 in it's place.
									IEnumerable parentCollection = PeekAtObjectStack() ?? null;

									if (parentCollection == null)
									{
										break; // early out, we can't the vector to anyone!
									}
									else if (parentCollection is Dictionary<string, object>)
									{
										var parentDict = (Dictionary<string, object>)parentCollection;
										var myKey = parentDict.FirstOrDefault(o => o.Value == potentialVector3Dict).Key ?? null;
										if (myKey != null)
											parentDict[myKey] = newVector;
									}
									else if (parentCollection is List<object>)
									{
										var parentAry = (List<object>)parentCollection;
										var myIndex = parentAry.IndexOf(potentialVector3Dict);
										if (myIndex >= 0)
											parentAry[myIndex] = newVector;
									}
								}

								// Clear the tracker regardless
								vector3Tracker = null;
								break;

							case JsonToken.EndArray:
								// Pop the last object on the stack as we should no longer add to it.
								objectStack.Pop();
								break;

							default:
								Console.WriteLine("Unhandled control token: {0}", reader.TokenType);
								break;
						}
					}
					else
					{
						// This is an item token, something with a value

						switch (reader.TokenType)
						{
							case JsonToken.PropertyName:
								nextProperty = (string)reader.Value;
								break;

							case JsonToken.Float:
								// Special case, we need to check for Vector3's
								if (vector3Tracker != null && vector3Tracker == nextProperty)
								{
									switch (vector3Tracker)
									{
										case "x":
											vector3Tracker = "y";
											break;
										case "y":
											vector3Tracker = "z";
											break;
										case "z":
											vector3Tracker = "yes";
											break;
									}
								}
								else
								{
									vector3Tracker = null;
								}

								// Still add to the current dictionary
								AddValueToParent((float)(double)reader.Value);
								break;

							case JsonToken.Boolean:
							case JsonToken.String:
							case JsonToken.Integer:
								// Clear the Vector3 tracker if we couldn't possibly be part of a vector.
								//if (vector3Tracker != null)
								vector3Tracker = null;

								// Add to the parent object
								AddValueToParent(reader.Value);
								break;

							default:
								Console.WriteLine("Unhandled value token: {0}", reader.TokenType);
								break;
						}
					}
				}
			} 
			catch (Exception e)
			{
				Debug.WriteLine($"UGC Parser Error\n{e}");
			}
		}
	}*/

/*
namespace racing
{
	public static partial class UGC
	{
		/// <summary>
		/// This empty <see cref="JArray"/> is used as a placeholder for missing data when reading from UGC
		/// </summary>
		internal static JArray missingData = new JArray();

		internal static bool ContainsKeys(this JObject o, params string[] keys)
		{
			return keys.All(key => o.ContainsKey(key));
		}

		internal static object TryGet(this JObject o, string key, object defaultValue)
		{
			return o.ContainsKey(key) ? o[key] : defaultValue;
		}

		internal static JArray TryGetArray(this JObject o, string key, JArray defaultValue = null)
		{
			if (o.ContainsKey(key))
			{
				var potentialArray = o[key];
				if (potentialArray.Type == JTokenType.Array)
					return (JArray)potentialArray;
			}

			return defaultValue ?? null;
		}

		public static Vector3 ToVector3(this JToken t)
		{
			if (t.Type == JTokenType.Object)
			{
				var o = (JObject)t;
				if (o.HasValues && o.ContainsKeys("x", "y", "z"))
					return new Vector3((float)o["x"], (float)o["y"], (float)o["z"]);
			}

			return Vector3.Zero;
		}

		[Serializable]
		public struct CheckpointDefinition
		{
			public Vector3 Location;
			public float Heading;
			public float Scale;
			public bool IsRound;
			//public int WrongWayTimer;
			//public CheckpointDefinition SecondCheckpoint;


			/* // All values relating to checkpoints
			Var10 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chh");		// Heading
			iVar11 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chs");		// Scale
			iVar12 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chs2");		// Scale (2)
			iVar13 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chvs");		//
			iVar14 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chpp");		//
			iVar15 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chpps");		//
			iVar16 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chl");		// Location
			iVar17 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "sndchk");	// Location (2)
			iVar18 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "sndrsp");	// 
			iVar19 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpwwt");		// Wrong Way Time
			iVar20 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cppsst");	
			iVar21 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpado");
			iVar22 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpados");
			iVar23 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chttu");
			iVar24 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chttr");
			iVar25 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpbs1");		
			iVar26 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpbs2");
			iVar27 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cptfrm");
			iVar28 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cptfrms");
			iVar29 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "trfmvm");
			iVar30 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chdlo");
			iVar31 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chsto");
			iVar32 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chdlos");
			iVar33 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "chstos");
			iVar34 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "rsp");
			iVar35 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cdsblcu");
			iVar36 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpdss");
			iVar37 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "rndchk");	// Is Round
			iVar38 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "rndchks");	// Is Round (2)
			iVar39 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpwtr");
			iVar40 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpwtrs");
			iVar41 = DATAFILE::_OBJECT_VALUE_GET_ARRAY(iVar2, "cpair");		// Has second checkpoint
			*/

/* // Old Constructors
public CheckpointDefinition(Vector3 l, float h, float s, bool iR)
{
	Location = l;
	Heading = h;
	Scale = s;
	IsRound = iR;
}


public CheckpointDefinition(dynamic obj)
{
	try
	{
		Location = obj.Location;
		Heading = obj.Heading;
		Scale = obj.Scale;
		IsRound = obj.IsRound;
	} catch(Exception e)
	{
		throw new ArgumentException("Given object did not contain the data necessary to construct a CheckpointDefinition", "obj", e);
	}
}
}

public struct UGCMap
{
public JObject raw { get; private set; }
public JObject Mission { get => (JObject)raw["mission"]; }
public JObject Race { get => (JObject)raw["mission"]["race"]; }
public JObject Prop { get => (JObject)raw["mission"]["prop"]; }
//public List<CheckpointDefinition> Checkpoints { get; private set; }
//public List<PropDefinition> Props { get; private set; }

public UGCMap(JObject rawData)
{
	raw = rawData;
	//Props = new List<PropDefinition>();
	//Checkpoints = new List<CheckpointDefinition>();


}
}

public static UGCMap ParseUGC(string ugcJsonString)
{
JObject rawData = JObject.Parse(ugcJsonString);
var race = new UGCMap(rawData);

return race;
}
}
}
*/
