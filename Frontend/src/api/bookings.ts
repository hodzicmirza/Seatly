import api from "./client";
import type { BookingResponse, CreateBookingRequest } from "@/types";

export async function createBooking(request: CreateBookingRequest): Promise<BookingResponse> {
  const { data } = await api.post<BookingResponse>("/bookings", request);
  return data;
}

export async function getMyBookings(): Promise<BookingResponse[]> {
  const { data } = await api.get<BookingResponse[]>("/bookings/my-bookings");
  return data;
}

export async function getBookingById(id: string): Promise<BookingResponse> {
  const { data } = await api.get<BookingResponse>(`/bookings/${id}`);
  return data;
}

export async function cancelBooking(id: string): Promise<void> {
  await api.delete(`/bookings/${id}`);
}

export async function markBookingAsUsed(id: string, qrCodeData: string): Promise<void> {
  await api.post(`/bookings/${id}/use`, { qrCodeData });
}
