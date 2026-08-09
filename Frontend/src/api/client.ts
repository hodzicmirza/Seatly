import axios from "axios";
import { supabase } from "@/lib/supabase";

const baseURL =
  import.meta.env.VITE_API_BASE_URL || "https://seatlybackend.onrender.com/api";

const api = axios.create({
  baseURL,
});

api.interceptors.request.use(async (config) => {
  const {
    data: { session },
  } = await supabase.auth.getSession();
  if (session?.access_token) {
    config.headers.Authorization = `Bearer ${session.access_token}`;
  }
  return config;
});

export default api;
