import { useState } from "react"
import { Users, Search } from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Input } from "@/components/ui/input"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { useApplicants } from "@/hooks/useApi"

function formatZAR(amount: number) {
  return new Intl.NumberFormat("en-ZA", {
    style: "currency",
    currency: "ZAR",
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(amount)
}

function EmploymentBadge({ status }: { status: string }) {
  const variant = status === "Employed" ? "success" : status === "Self-employed" ? "info" : "warning"
  return <Badge variant={variant}>{status}</Badge>
}

export default function Applicants() {
  const [search, setSearch] = useState("")
  const { data: applicants, loading } = useApplicants()

  const filtered = applicants.filter(
    (a) =>
      a.firstName.toLowerCase().includes(search.toLowerCase()) ||
      a.lastName.toLowerCase().includes(search.toLowerCase()) ||
      a.email.toLowerCase().includes(search.toLowerCase())
  )

  return (
    <div className="space-y-6">
      <div className="flex items-end justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Applicants</h1>
          <p className="text-muted-foreground mt-1">Manage applicant profiles and information</p>
        </div>
      </div>

      <Card className="border-border/50">
        <CardHeader className="pb-4">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <CardTitle>All Applicants</CardTitle>
              <CardDescription>{filtered.length} applicants found</CardDescription>
            </div>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Search applicants..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="pl-9 w-[260px]"
              />
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow className="hover:bg-transparent">
                <TableHead>Name</TableHead>
                <TableHead>ID Number</TableHead>
                <TableHead>Phone</TableHead>
                <TableHead>Employment</TableHead>
                <TableHead>Income</TableHead>
                <TableHead>Expenses</TableHead>
                <TableHead>DTI Ratio</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                Array.from({ length: 5 }).map((_, i) => (
                  <TableRow key={i}>
                    <TableCell colSpan={7}>
                      <div className="h-10 bg-muted animate-pulse rounded" />
                    </TableCell>
                  </TableRow>
                ))
              ) : filtered.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} className="text-center py-12 text-muted-foreground">
                    <Users className="mx-auto h-10 w-10 mb-3 opacity-30" />
                    No applicants found
                  </TableCell>
                </TableRow>
              ) : (
                filtered.map((applicant) => {
                  const dti = ((applicant.monthlyExpenses / applicant.monthlyIncome) * 100).toFixed(1)
                  return (
                    <TableRow key={applicant.id}>
                      <TableCell>
                        <div className="flex items-center gap-3">
                          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center text-primary text-xs font-semibold">
                            {applicant.firstName.charAt(0)}{applicant.lastName.charAt(0)}
                          </div>
                          <div>
                            <p className="text-sm font-medium">{applicant.firstName} {applicant.lastName}</p>
                            <p className="text-xs text-muted-foreground">{applicant.email}</p>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell className="font-mono text-xs">{applicant.idNumber}</TableCell>
                      <TableCell className="text-sm">{applicant.phoneNumber}</TableCell>
                      <TableCell>
                        <EmploymentBadge status={applicant.employmentStatus} />
                      </TableCell>
                      <TableCell className="font-medium">{formatZAR(applicant.monthlyIncome)}</TableCell>
                      <TableCell className="text-muted-foreground">{formatZAR(applicant.monthlyExpenses)}</TableCell>
                      <TableCell>
                        <span className={Number(dti) > 40 ? "text-rose-400 font-medium" : "text-emerald-400 font-medium"}>
                          {dti}%
                        </span>
                      </TableCell>
                    </TableRow>
                  )
                })
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  )
}
