import type {
    CalculateTollRequest,
    CalculateTollResponse
} from "@/types/toll";

const apiUrl = process.env.NEXT_PUBLIC_API_URL;

export async function calculateTollFee(
    request: CalculateTollRequest,
): Promise<CalculateTollResponse> {
    if (!apiUrl) 
    {
        throw new Error("The API URL has not been configured");
    }

    const response = await fetch(`${apiUrl}/api/toll/calculate`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(request),
    });

    if (!response.ok) {
        const errorResponse = await response
            .json()
            .catch(() => null);
        
        throw new Error(
            errorResponse?.error ??
                "The toll fee could not be calculated."
        );
    }

    return response.json() as Promise<CalculateTollResponse>;
}