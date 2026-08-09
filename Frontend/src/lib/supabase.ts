import { createClient } from "@supabase/supabase-js";

const supabaseUrl =
  import.meta.env.VITE_SUPABASE_URL || "https://ffldjamngiojwakaqsvs.supabase.co";
const supabaseAnonKey =
  import.meta.env.VITE_SUPABASE_ANON_KEY ||
  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImZmbGRqYW1uZ2lvandha2Fxc3ZzIiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODYxMjQwOTIsImV4cCI6MjEwMTcwMDA5Mn0.hEmpCubDYiTvo7ArWKxKoYNay8iqFXsQaO0KzOfhQGs";

export const supabase = createClient(supabaseUrl, supabaseAnonKey);
