import { useState } from "react";
import ServiceResult from "@/types/app/ServiceResult";

export default function useApiHandler<T>(serviceMethod: () => Promise<ServiceResult<T>>) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [data, setData] = useState<T | null>(null);

  const handleRequest = async () => {
    setLoading(true);
    setError(null);
    const result = await serviceMethod();
    setLoading(false);
    if (!result.success) {
      setError(result.message);
    } else {
      setData(result.data ?? null);
    }
  };

  return { data, error, loading, handleRequest };
}
