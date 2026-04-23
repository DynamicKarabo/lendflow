const API_BASE = import.meta.env.VITE_API_URL || "/api/v1";

function getToken(): string | null {
  return localStorage.getItem("lendflow_token");
}

async function fetchJson<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const token = getToken();
  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  });

  if (!res.ok) {
    const body = await res.text();
    throw new Error(`HTTP ${res.status}: ${body}`);
  }

  if (res.status === 204) {
    return undefined as T;
  }

  return res.json() as Promise<T>;
}

export const api = {
  auth: {
    login: (Email: string, Password: string) =>
      fetchJson<import("@/types").LoginResponse>("/auth/login", {
        method: "POST",
        body: JSON.stringify({ Email, Password }),
      }),
  },

  applicants: {
    list: () => fetchJson<import("@/types").Applicant[]>("/applicants"),
    get: (id: string) =>
      fetchJson<import("@/types").Applicant>(`/applicants/${id}`),
    create: (data: Omit<import("@/types").Applicant, "Id" | "TenantId" | "CreatedAt" | "UpdatedAt"> & { IdempotencyKey: string }) =>
      fetchJson<{ ApplicantId: string }>("/applicants", {
        method: "POST",
        body: JSON.stringify(data),
      }),
  },

  applications: {
    list: (status?: string, pageNumber = 1, pageSize = 20) => {
      const params = new URLSearchParams();
      if (status && status !== "all") params.append("status", status);
      params.append("pageNumber", String(pageNumber));
      params.append("pageSize", String(pageSize));
      return fetchJson<import("@/types").PagedResult<import("@/types").LoanApplication>>(`/applications?${params}`);
    },
    get: (id: string) =>
      fetchJson<import("@/types").LoanApplication>(`/applications/${id}`),
    assess: (id: string) =>
      fetchJson<unknown>(`/applications/${id}/assess`, { method: "POST" }),
    decide: (id: string, Decision: string, Reason: string) =>
      fetchJson<unknown>(`/applications/${id}/decision`, {
        method: "POST",
        body: JSON.stringify({ Decision, Reason }),
      }),
  },

  loans: {
    list: (status?: string, pageNumber = 1, pageSize = 20) => {
      const params = new URLSearchParams();
      if (status && status !== "all") params.append("status", status);
      params.append("pageNumber", String(pageNumber));
      params.append("pageSize", String(pageSize));
      return fetchJson<import("@/types").PagedResult<import("@/types").Loan>>(`/loans?${params}`);
    },
    get: (id: string) =>
      fetchJson<import("@/types").Loan>(`/loans/${id}`),
    repayments: (id: string) =>
      fetchJson<import("@/types").Repayment[]>(`/loans/${id}/repayments`),
  },
};
