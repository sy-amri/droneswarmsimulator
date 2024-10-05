// See https://aka.ms/new-console-template for more information
using System;
class HelloWorld {
  static void Main() {
      
      int numRepeat = 100;
      int max = 15000;
      int min = 100;
      int stepsize = 100;
      int numsteps = (max-min) / stepsize;

      // Arrays to store average timings for each function
      float[] timeMax = new float[numsteps];
      float[] timeMin = new float[numsteps];
      float[] timeAverage = new float[numsteps];
      float[] timePrint = new float[numsteps];
      float[] timeBubbleSort = new float[numsteps];

      for (int i = 0; i < numsteps; i++)
        {
            int numdrones = i * stepsize + min;
            Console.WriteLine("Current num drones = " + numdrones);

            Flock flock = new Flock(numdrones);
            flock.Init(numdrones); // Initialize flock with numdrones

            var watch = new System.Diagnostics.Stopwatch();

            // Timing for max()
            watch.Restart();
            for (int rep = 0; rep < numRepeat; rep++)
            {
                flock.max();
            }
            watch.Stop();
            timeMax[i] = watch.ElapsedMilliseconds / (float)numRepeat;

            // Timing for min()
            watch.Restart();
            for (int rep = 0; rep < numRepeat; rep++)
            {
                flock.max();
            }
            watch.Stop();
            timeMin[i] = watch.ElapsedMilliseconds / (float)numRepeat;

            // Timing for average()
            watch.Start();
            for (int rep = 0; rep < numRepeat; rep++)
            {
                flock.average();
            }
            watch.Stop();
            timeAverage[i] = watch.ElapsedMilliseconds / (float)numRepeat;

            // Timing for print()
            watch.Restart();
            for (int rep = 0; rep < numRepeat; rep++)
            {
                flock.print();
            }
            watch.Stop();
            timePrint[i] = watch.ElapsedMilliseconds / (float)numRepeat;

            // Timing for Bubble sort
            watch.Restart();
            for (int rep = 0; rep < numRepeat; rep++)
            {
                flock.bubblesort();
            }
            watch.Stop();
            timeBubbleSort[i] = watch.ElapsedMilliseconds / (float)numRepeat;
        }
      
      // write results to csv files
      // see https://www.csharptutorial.net/csharp-file/csharp-write-csv-files/

      using (StreamWriter file = new StreamWriter("results.csv"))
        {
            file.WriteLine("NumDrones,MaxTime,MinTime,AverageTime,PrintTime,BubbleSortTime");
            for (int i = 0; i < numsteps; i++)
            {
                int numdrones = i * stepsize + min;
                file.WriteLine($"{numdrones},{timeMax[i]},{timeMin[i]},{timeAverage[i]},{timePrint[i]},{timeBubbleSort[i]}");
            }
        }

        Console.WriteLine("Results written to results.csv");
  }
}
