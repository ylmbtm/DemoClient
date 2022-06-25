using System;
using System.Collections.Generic;

namespace MapMagic
{
	//surprisingly Unity's ArrayUtility is an Editor calss
	//so this is it's analog to use in builds
	
	public static class ArrayTools 
	{
		#region Array

			public static int Find(this Array array, object obj)
			{
				for (int i=0; i<array.Length; i++)
					if (array.GetValue(i) == obj) return i;
				return -1;
			}

			static public int Find<T> (T[] array, T obj) where T : class
			{
				for (int i=0; i<array.Length; i++)
					if (array[i] == obj) return i;
				return -1;
			}
			

			static public void RemoveAt<T> (ref T[] array, int num) { array = RemoveAt(array, num); }
			static public T[] RemoveAt<T> (T[] array, int num)
			{
				T[] newArray = new T[array.Length-1];
				for (int i=0; i<newArray.Length; i++) 
				{
					if (i<num) newArray[i] = array[i];
					else newArray[i] = array[i+1];
				}
				return newArray;
			}

			static public void Remove<T> (ref T[] array, T obj) where T : class {array = Remove(array, obj); }
			static public T[] Remove<T> (T[] array, T obj) where T : class
			{
				int num = Find<T>(array, obj);
				return RemoveAt<T>(array,num);
			}
			

			static public void Add<T> (ref T[] array, int after, T element=default(T)) { array = Add(array, after, element:element); }
			static public T[] Add<T> (T[] array, int after, T element=default(T))
			{
				if (array==null || array.Length==0) { return new T[] {element}; }
				if (after > array.Length-1) after = array.Length-1;
				
				T[] newArray = new T[array.Length+1];
				for (int i=0; i<newArray.Length; i++) 
				{
					if (i<=after) newArray[i] = array[i];
					else if (i == after+1) newArray[i] = element;
					else newArray[i] = array[i-1];
				}
				return newArray;
			}
			static public T[] Add<T> (T[] array, T element=default(T)) { return Add(array, array.Length-1, element); }
			static public void Add<T> (ref T[] array, T element=default(T)) { array = Add(array, array.Length-1, element); }

			static public void Resize<T> (ref T[] array, int newSize, T element=default(T)) { array = Resize(array, newSize, element); }
			static public T[] Resize<T> (T[] array, int newSize, T element=default(T))
			{
				//NOTE: element is not unique. On adding 2 items it will fill both with one instance
				if (array.Length == newSize) return array;
				else if (newSize > array.Length) 
				{ 
					array = Add(array, element); 
					array = Resize(array,newSize); 
					return array; 
					}
				else 
				{ 
					array = RemoveAt(array, array.Length-1); 
					array = Resize(array,newSize); 
					return array;
				}
			}

			static public void Append<T> (ref T[] array, T[] additional) { array = Append(array, additional); }
			static public T[] Append<T> (T[] array, T[] additional)
			{
				T[] newArray = new T[array.Length+additional.Length];
				for (int i=0; i<array.Length; i++) { newArray[i] = array[i]; }
				for (int i=0; i<additional.Length; i++) { newArray[i+array.Length] = additional[i]; }
				return newArray;
			}

			static public void Switch<T> (T[] array, int num1, int num2)
			{
				if (num1<0 || num1>=array.Length || num2<0 || num2 >=array.Length) return;
				
				T temp = array[num1];
				array[num1] = array[num2];
				array[num2] = temp;
			}

			static public void Switch<T> (T[] array, T obj1, T obj2) where T : class
			{
				int num1 = Find<T>(array, obj1);
				int num2 = Find<T>(array, obj2);
				Switch<T>(array, num1, num2);
			}

			static public T[] Truncated<T> (this T[] src, int length)
			{
				T[] dst = new T[length];
				for (int i=0; i<length; i++) dst[i] = src[i];
				return dst;
			}
		#endregion

		#region Array Sorting

			static public void QSort (float[] array) { QSort(array, 0, array.Length-1); }
			static public void QSort (float[] array, int l, int r)
			{
				float mid = array[l + (r-l) / 2]; //(l+r)/2
				int i = l;
				int j = r;
				
				while (i <= j)
				{
					while (array[i] < mid) i++;
					while (array[j] > mid) j--;
					if (i <= j)
					{
						float temp = array[i];
						array[i] = array[j];
						array[j] = temp;
						
						i++; j--;
					}
				}
				if (i < r) QSort(array, i, r);
				if (l < j) QSort(array, l, j);
			}

			static public void QSort<T> (T[] array, float[] reference) { QSort(array, reference, 0, reference.Length-1); }
			static public void QSort<T> (T[] array, float[] reference, int l, int r)
			{
				float mid = reference[l + (r-l) / 2]; //(l+r)/2
				int i = l;
				int j = r;
				
				while (i <= j)
				{
					while (reference[i] < mid) i++;
					while (reference[j] > mid) j--;
					if (i <= j)
					{
						float temp = reference[i];
						reference[i] = reference[j];
						reference[j] = temp;

						T tempT = array[i];
						array[i] = array[j];
						array[j] = tempT;
						
						i++; j--;
					}
				}
				if (i < r) QSort(array, reference, i, r);
				if (l < j) QSort(array, reference, l, j);
			}

			static public void QSort<T> (List<T> list, float[] reference) { QSort(list, reference, 0, reference.Length-1); }
			static public void QSort<T> (List<T> list, float[] reference, int l, int r)
			{
				float mid = reference[l + (r-l) / 2]; //(l+r)/2
				int i = l;
				int j = r;
				
				while (i <= j)
				{
					while (reference[i] < mid) i++;
					while (reference[j] > mid) j--;
					if (i <= j)
					{
						float temp = reference[i];
						reference[i] = reference[j];
						reference[j] = temp;

						T tempT = list[i];
						list[i] = list[j];
						list[j] = tempT;
						
						i++; j--;
					}
				}
				if (i < r) QSort(list, reference, i, r);
				if (l < j) QSort(list, reference, l, j);
			}

			static public int[] Order (int[] array, int[] order=null, int max=0, int steps=1000000, int[] stepsArray=null) //returns an order int array
			{
				if (max==0) max=array.Length;
				if (stepsArray==null) stepsArray = new int[steps+1];
				else steps = stepsArray.Length-1;
			
				//creating starts array
				int[] starts = new int[steps+1];
				for (int i=0; i<max; i++) starts[ array[i] ]++;
					
				//making starts absolute
				int prev = 0;
				for (int i=0; i<starts.Length; i++)
					{ starts[i] += prev; prev = starts[i]; }

				//shifting starts
				for (int i=starts.Length-1; i>0; i--)
					{ starts[i] = starts[i-1]; }  
				starts[0] = 0;

				//using magic to compile order
				if (order==null) order = new int[max];
				for (int i=0; i<max; i++)
				{
					int h = array[i]; //aka height
					int num = starts[h];
					order[num] = i;
					starts[h]++;
				}
				return order;
			}

			static public T[] Convert<T,Y> (Y[] src)
			{
				T[] result = new T[src.Length];
				for (int i=0; i<src.Length; i++) result[i] = (T)(object)(src[i]);
				return result;
			}

		#endregion
	}

}
