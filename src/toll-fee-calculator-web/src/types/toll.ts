export const vehicleTypes = [
    "Car",
    "Motorbike",
    "Tractor",
    "Emergency",
    "Diplomat",
    "Foreign",
    "Military",
  ] as const;
  
  export type VehicleType =
    (typeof vehicleTypes)[number];
  
  export interface CalculateTollRequest {
    vehicleType: VehicleType;
    passages: string[];
  }
  
  export interface TollPassageResponse {
    passageTime: string;
    passageFee: number;
    runningTotal: number;
    chargePeriodNumber: number;
    startsNewChargePeriod: boolean;
    dailyCapReached: boolean;
  }
  
  export interface CalculateTollResponse {
    totalFee: number;
    maximumDailyFee: number;
    singleChargePeriodMinutes: number;
    passages: TollPassageResponse[];
  }