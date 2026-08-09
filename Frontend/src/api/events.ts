import api from "./client";
import type { EventResponse, CreateEventRequest } from "@/types";

export async function getAllEvents(): Promise<EventResponse[]> {
  const { data } = await api.get<EventResponse[]>("/events");
  return data;
}

export async function getEventById(id: string): Promise<EventResponse> {
  const { data } = await api.get<EventResponse>(`/events/${id}`);
  return data;
}

export async function searchEvents(params: {
  name?: string;
  from?: string;
  to?: string;
  eventType?: string;
}): Promise<EventResponse[]> {
  const { data } = await api.get<EventResponse[]>("/events/search", { params });
  return data;
}

export async function createEvent(request: CreateEventRequest): Promise<EventResponse> {
  const { data } = await api.post<EventResponse>("/events", request);
  return data;
}

export async function updateEvent(id: string, request: Partial<CreateEventRequest>): Promise<EventResponse> {
  const { data } = await api.put<EventResponse>(`/events/${id}`, request);
  return data;
}

export async function deleteEvent(id: string): Promise<void> {
  await api.delete(`/events/${id}`);
}
