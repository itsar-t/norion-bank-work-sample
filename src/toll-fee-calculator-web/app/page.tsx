//Tell Next.js that this is a Client Component without it Next.js App Router would treat it as a Server Component
"use client"; 


import { type SubmitEvent, useState } from "react";
import { calculateTollFee } from "@/lib/toll-api";

import {
  type CalculateTollResponse,
  type VehicleType,
  vehicleTypes,
} from "@/types/toll";

import {
  TollVisualization,
} from "@/components/toll-calculator/TollVisualization";

//datetime-local expects the form ÅÅÅÅ-MM-DDTHH:mm
const initialPassage = "2013-01-02T06:10";

export default function Home() {
  const [vehicleType, setVehicleType] = useState<VehicleType>("Car");
  const [passages, setPassages] = useState<string[]>([initialPassage]);
  const [calculationResult, setCalculationResult] = useState<CalculateTollResponse | null>(null);

  //Potential Error
  const [error, setError] = useState<string | null>(null);

  //API call ongoing
  const [isLoading, setIsLoading] = useState(false);

  // function to add a new passage to our string array with passages
  // will execute when user presses + Add passage and will add a new passage containing the initialPassage set above
  function addPassage() {
    setPassages((currentPassages) => [
      ...currentPassages,
      initialPassage,
    ]);
  }

  function updatePassage(
    index: number,
    value: string,
  ) {
    setPassages((currentPassages) =>
      currentPassages.map((passage, passageIndex) =>
        passageIndex === index ? value : passage,
      ),
    );
  }

  function removePassage(index: number) {
    setPassages((currentPassages) =>
      currentPassages.filter(
        (_, passageIndex) => passageIndex !== index,
      ),
    );
  }

  async function handleSubmit(
    event: SubmitEvent<HTMLFormElement>,
  ) {
    event.preventDefault();
    setError(null);
    setCalculationResult(null);
    setIsLoading(true);

    try {
      const result = await calculateTollFee({
        vehicleType,
        passages
      });

      setCalculationResult(result);
    } catch (caughtError) {
      setError(
        caughtError instanceof Error
          ? caughtError.message
          : "An unexpected error occured.",
      );
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <main className="min-h-screen bg-slate-950 px-6 py-12 text-white">
      <div className="mx-auto max-w-2xl">
        <header className="mb-10">
          <p className="mb-2 text-sm font-semibold uppercase tracking-widest text-cyan-400">
            Gothenburg toll calculator
          </p>
          <h1 className="text-4xl font-bold tracking-tight">
            Calculate your toll fee
          </h1>
          <p className="mt-4 text-slate-300">
            Select a vehicle and add its passages for one day.
          </p>
        </header>
        <form
          onSubmit={handleSubmit}
          className="space-y-8 rounded-2xl border border-slate-800 bg-slate-900 p-6 shadow-2xl"
        >
          <div>
            <label
              htmlFor="vehicle-type"
              className="mb-2 block font-medium">
              Vehicle type
            </label>
            <select
              id="vehicle-type"
              value={vehicleType}
              onChange={(event) =>
                setVehicleType(
                  event.target.value as VehicleType,
                )
              }
              className="w-full rounded-lg border border-slate700 bg-slate-950 px-4 py-3"
            >
              {vehicleTypes.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>

          <fieldset>
            <legend className="mb-3 font-medium">
              Passages
            </legend>

            <div className="spece-y-3">
              {passages.map((passage, index) => (
                <div
                  key={index}
                  className="flex flex-col gap-3 sm:flex-row">
                   
                  <input
                    type="datetime-local"
                    value={passage}
                    onChange={(event) =>
                      updatePassage(
                        index,
                        event.target.value,
                      )
                    }
                    required
                    className="flex-1 rounded-lg border border-slate-700 bg-slate-950 px-4 py-3 scheme-dark"
                    
                  />
                  
                  <button
                    type="button"
                    onClick={() => removePassage(index)}
                    disabled={passages.length === 1}
                    className="rounded-lg border border-slate-700 px-4 py-3 transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-40 "
                  >Remove</button>
                </div>
              ))}
             
            </div>
            <button
              type="button"
              onClick={addPassage}
              className="mt-4 text-sm font-semibold text-cyan-400 hover:text-cyan-300"
            >
              + Add passage
            </button>
          </fieldset>

          <button
            type="submit"
            disabled={isLoading}
            className="w-full rounded-lg bg-cyan-400 px-5 py-3 font-bold text-slate-950 transition hover:bg-cyan-300 disabled:cursor-wait disabled:opacity-60"
          >
            {isLoading
              ? "Calculating..."
              : "Calculate toll fee"}
          </button>
        </form>
        
        {calculationResult !== null && (
          <TollVisualization
            calculation={calculationResult}
          />
        )}
        
        
        {error && (
          <p
            role="alert"
            className="mt-6 rounded-xl border border-red-500/30 bg-red-500/10 p-4 text-red-200"
          >
            {error}
          </p>
        )}
      </div>
    </main>
  )
}