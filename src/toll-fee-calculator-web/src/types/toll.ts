export const vehicleTypes = [
    "Car",
    "Motorbike",
    "Tractor",
    "Emergency",
    "Diplomat",
    "Foreign",
    "Military",
] as const;

export type VehicleType = (typeof vehicleTypes)[number];

export interface CalculateTollRequest {
    vehicleType: VehicleType;
    passages: string[];
}

export interface CalculateTollResponse {
    totalFee: number;
  }