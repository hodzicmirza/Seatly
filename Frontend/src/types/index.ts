export interface CategoryResponse {
  name: string;
  priceMultiplier: number;
  finalPrice: number;
  seatsCount: number;
}

export interface EventResponse {
  id: string;
  name: string;
  description: string;
  date: string;
  city: string;
  country: string;
  basePrice: number;
  totalSeats: number;
  availableSeats: number;
  eventType: string;
  categories: CategoryResponse[];
}

export interface CreateEventRequest {
  name: string;
  description: string;
  date: string;
  street: string;
  city: string;
  country: string;
  basePrice: number;
  totalSeats: number;
  categories: { name: string; multiplier: number; seatsCount: number }[];
  eventType: number;
  headliner?: string;
  supportAct?: string;
  organizer?: string;
  keynoteSpeaker?: string;
}

export interface BookingResponse {
  bookingId: string;
  eventName: string;
  eventDate: string;
  eventLocation: string;
  category: string;
  numberOfSeats: number;
  totalPrice: number;
  status: string;
  qrCodeBase64?: string;
  createdAt: string;
}

export interface CreateBookingRequest {
  eventId: string;
  numberOfSeats: number;
  categoryName: string;
}
