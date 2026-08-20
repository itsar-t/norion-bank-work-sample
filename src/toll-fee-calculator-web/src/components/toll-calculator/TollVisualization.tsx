"use client";

import { useEffect, useState } from "react";
import {
  type CalculateTollResponse,
} from "@/types/toll";

interface TollVisualizationProps {
  calculation: CalculateTollResponse;
}

const PASSAGE_DURATION_MS = 1800;
const ROAD_START_POSITION = 92;
const ROAD_END_POSITION = 8;

function calculateRoadPosition(
  index: number,
  lastIndex: number,
) {
  if (lastIndex === 0) {
    return 50;
  }

  const progress = index / lastIndex;

  const roadLength =
    ROAD_START_POSITION - ROAD_END_POSITION;

  return ROAD_START_POSITION - progress * roadLength;
}

export function TollVisualization({
  calculation,
}: TollVisualizationProps) {
  const [activePassageIndex, setActivePassageIndex] =
    useState(0);

  useEffect(() => {
    if (calculation.passages.length <= 1) {
      return;
    }

    const intervalId = window.setInterval(() => {
      setActivePassageIndex((currentIndex) => {
        const lastPassageIndex =
          calculation.passages.length - 1;

        if (currentIndex >= lastPassageIndex) {
          window.clearInterval(intervalId);
          return currentIndex;
        }

        return currentIndex + 1;
      });
    }, PASSAGE_DURATION_MS);

    return () => {
      window.clearInterval(intervalId);
    };
  }, [calculation.passages.length]);

  const activePassage =
    calculation.passages[activePassageIndex];

  if (!activePassage) {
    return null;
  }

  const formattedPassageTime =
    new Date(activePassage.passageTime)
      .toLocaleTimeString("sv-SE", {
        hour: "2-digit",
        minute: "2-digit",
      });

  const lastPassageIndex =
    calculation.passages.length - 1;

  const isFirstPassage =
    activePassageIndex === 0;

  const isAnimationComplete =
    activePassageIndex === lastPassageIndex;

  const carPosition = calculateRoadPosition(
    activePassageIndex,
    lastPassageIndex,
  );

  const dailyCapProgress =
    calculation.maximumDailyFee === 0
      ? 100
      : Math.min(
          (activePassage.runningTotal /
            calculation.maximumDailyFee) *
            100,
          100,
        );

  let chargePeriodMessage =
    `Within the current ${calculation.singleChargePeriodMinutes}-minute period. ` +
    "Only the highest passage fee is charged.";

  if (isFirstPassage) {
    chargePeriodMessage =
      "The first charge period has started.";
  } else if (activePassage.startsNewChargePeriod) {
    chargePeriodMessage =
      `A new charge period has started because more than ` +
      `${calculation.singleChargePeriodMinutes} minutes have passed.`;
  }

  return (
    <section
      aria-label="Toll passage visualization"
      className="mt-6 overflow-hidden rounded-2xl border border-slate-800 bg-slate-900 shadow-2xl"
    >
      <header className="flex flex-col gap-4 border-b border-slate-800 p-6 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <p className="text-sm font-semibold uppercase tracking-widest text-cyan-400">
            Live passage
          </p>

          <h2 className="mt-1 text-2xl font-bold">
            Passage {activePassageIndex + 1} of{" "}
            {calculation.passages.length}
          </h2>
        </div>

        <div
          aria-live="polite"
          className="rounded-xl bg-slate-950 px-5 py-3 text-right"
        >
          <p className="text-xs uppercase tracking-wide text-slate-400">
            Current total
          </p>

          <p className="text-3xl font-bold text-emerald-300">
            {activePassage.runningTotal} SEK
          </p>
        </div>
      </header>

      <div className="relative h-64 overflow-hidden bg-gradient-to-b from-sky-900 via-sky-950 to-slate-950">
        <div className="absolute left-6 top-5 rounded-lg border border-cyan-400/30 bg-slate-950/80 px-4 py-2 backdrop-blur">
          <p className="text-xs uppercase tracking-wide text-slate-400">
            Passage time
          </p>

          <time
            dateTime={activePassage.passageTime}
            className="text-2xl font-bold text-cyan-300"
          >
            {formattedPassageTime}
          </time>
        </div>

        <div
          aria-hidden="true"
          className="absolute bottom-20 left-1/2 h-28 w-32 -translate-x-1/2"
        >
          <div className="absolute bottom-0 left-1 h-24 w-3 rounded-t bg-slate-300" />

          <div className="absolute bottom-0 right-1 h-24 w-3 rounded-t bg-slate-300" />

          <div className="absolute left-0 right-0 top-2 flex h-10 items-center justify-center rounded bg-cyan-400 font-black tracking-widest text-slate-950 shadow-[0_0_24px_rgba(34,211,238,0.45)]">
            TOLL
          </div>

          <div className="absolute left-4 right-4 top-14 h-1 bg-red-400 shadow-[0_0_12px_rgba(248,113,113,0.8)]" />
        </div>

        <div
          aria-hidden="true"
          className="absolute bottom-0 left-0 right-0 h-24 bg-slate-800"
        >
          <div className="absolute left-0 right-0 top-1/2 border-t-4 border-dashed border-amber-200/80" />
        </div>

        <div
          role="img"
          aria-label={`Car passing the toll station at ${formattedPassageTime}`}
          style={{
            left: `calc(${carPosition}% - 1.5rem)`,
          }}
          className="absolute bottom-10 z-10 text-5xl transition-[left] duration-1000 ease-in-out motion-reduce:transition-none"
        >
          🚗
        </div>

        <div
          aria-hidden="true"
          className="absolute bottom-4 left-6 right-6"
        >
          {calculation.passages.map((passage, index) => {
            const markerPosition =
              calculateRoadPosition(
                index,
                lastPassageIndex,
              );

            const isProcessed =
              index <= activePassageIndex;

            return (
              <span
                key={`${passage.passageTime}-${index}`}
                style={{
                  left: `${markerPosition}%`,
                }}
                className={`absolute h-3 w-3 -translate-x-1/2 rounded-full border-2 transition-colors ${
                  isProcessed
                    ? "border-cyan-200 bg-cyan-400"
                    : "border-slate-500 bg-slate-800"
                }`}
              />
            );
          })}
        </div>
      </div>

      <div
        aria-live="polite"
        className="border-b border-slate-800 p-6"
      >
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <span className="inline-flex rounded-full border border-cyan-400/30 bg-cyan-400/10 px-3 py-1 text-xs font-bold uppercase tracking-wide text-cyan-300">
              Charge period{" "}
              {activePassage.chargePeriodNumber}
            </span>

            <p className="mt-3 text-sm text-slate-300">
              {chargePeriodMessage}
            </p>
          </div>

          {activePassage.startsNewChargePeriod &&
            !isFirstPassage && (
              <span className="inline-flex w-fit rounded-full border border-amber-400/30 bg-amber-400/10 px-3 py-2 text-sm font-semibold text-amber-300">
                New charge period
              </span>
            )}
        </div>
      </div>

      <div className="grid gap-4 p-6 sm:grid-cols-3">
        <div className="rounded-xl bg-slate-950 p-4">
          <p className="text-xs uppercase tracking-wide text-slate-400">
            Passage fee
          </p>

          <p className="mt-1 text-2xl font-bold">
            {activePassage.passageFee} SEK
          </p>
        </div>

        <div className="rounded-xl bg-slate-950 p-4">
          <p className="text-xs uppercase tracking-wide text-slate-400">
            Running total
          </p>

          <p className="mt-1 text-2xl font-bold text-cyan-300">
            {activePassage.runningTotal} SEK
          </p>
        </div>

        <div className="rounded-xl bg-slate-950 p-4">
          <p className="text-xs uppercase tracking-wide text-slate-400">
            Daily total
          </p>

          <p className="mt-1 text-2xl font-bold text-emerald-300">
            {isAnimationComplete
              ? `${calculation.totalFee} SEK`
              : "Calculating..."}
          </p>
        </div>
      </div>

      <div className="px-6 pb-6">
        <div className="mb-2 flex items-center justify-between text-sm">
          <span className="text-slate-400">
            Daily cap
          </span>

          <span
            className={
              activePassage.dailyCapReached
                ? "font-semibold text-amber-300"
                : "text-slate-300"
            }
          >
            {activePassage.runningTotal} /{" "}
            {calculation.maximumDailyFee} SEK
          </span>
        </div>

        <div
          role="progressbar"
          aria-label="Progress towards the daily toll cap"
          aria-valuemin={0}
          aria-valuemax={calculation.maximumDailyFee}
          aria-valuenow={activePassage.runningTotal}
          className="h-3 overflow-hidden rounded-full bg-slate-800"
        >
          <div
            style={{
              width: `${dailyCapProgress}%`,
            }}
            className={`h-full rounded-full transition-[width] duration-700 ${
              activePassage.dailyCapReached
                ? "bg-amber-400"
                : "bg-cyan-400"
            }`}
          />
        </div>

        {activePassage.dailyCapReached && (
          <div
            role="status"
            className="mt-4 rounded-xl border border-amber-400/30 bg-amber-400/10 p-4 text-amber-200"
          >
            <p className="font-bold">
              Daily cap reached
            </p>

            <p className="mt-1 text-sm">
              Further passages will not increase the
              daily total above{" "}
              {calculation.maximumDailyFee} SEK.
            </p>
          </div>
        )}
      </div>

      <footer className="border-t border-slate-800 px-6 py-4">
        <p
          aria-live="polite"
          className="text-sm text-slate-400"
        >
          {isAnimationComplete
            ? "All passages have been processed."
            : "Processing the next passage..."}
        </p>
      </footer>
    </section>
  );
}