import {
  Banknote,
  TrendingUp,
  TrendingDown,
  Users,
  FileText,
  Activity,
} from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Progress } from "@/components/ui/progress"
import { useDashboardStats, useLoans, useApplications } from "@/hooks/useApi"
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
} from "recharts"

const COLORS = ["#10b981", "#0ea5e9", "#f59e0b", "#f43f5e", "#8b5cf6"]

function formatZAR(amount: number) {
  return new Intl.NumberFormat("en-ZA", {
    style: "currency",
    currency: "ZAR",
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(amount)
}

export default function Dashboard() {
  const { data: stats, loading: statsLoading } = useDashboardStats()
  const { data: loans } = useLoans()
  const { data: applications } = useApplications()

  const statusData = [
    { name: "Active", value: loans.filter((l) => l.Status === "Active").length },
    { name: "Pending", value: loans.filter((l) => l.Status === "PendingDisbursement").length },
    { name: "Settled", value: loans.filter((l) => l.Status === "Settled").length },
  ].filter((d) => d.value > 0)

  const monthlyData = [
    { month: "Oct", disbursed: 45000 },
    { month: "Nov", disbursed: 62000 },
    { month: "Dec", disbursed: 38000 },
    { month: "Jan", disbursed: 135000 },
    { month: "Feb", disbursed: 55000 },
    { month: "Mar", disbursed: 85000 },
  ]

  return (
    <div className="space-y-8">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
        <p className="text-muted-foreground mt-1">
          Overview of your lending portfolio and key metrics
        </p>
      </div>

      {/* KPI Grid */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <Card className="relative overflow-hidden border-border/50 bg-gradient-to-br from-card to-card/50">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Disbursed
            </CardTitle>
            <div className="h-8 w-8 rounded-lg bg-emerald-500/10 flex items-center justify-center">
              <Banknote className="h-4 w-4 text-emerald-500" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {statsLoading ? "—" : formatZAR(stats?.TotalDisbursed || 0)}
            </div>
            <div className="flex items-center gap-1 mt-1 text-xs text-emerald-400">
              <TrendingUp className="h-3 w-3" />
              <span>+12.5% from last month</span>
            </div>
          </CardContent>
        </Card>

        <Card className="relative overflow-hidden border-border/50 bg-gradient-to-br from-card to-card/50">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Outstanding Balance
            </CardTitle>
            <div className="h-8 w-8 rounded-lg bg-sky-500/10 flex items-center justify-center">
              <Activity className="h-4 w-4 text-sky-500" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {statsLoading ? "—" : formatZAR(stats?.TotalOutstanding || 0)}
            </div>
            <div className="flex items-center gap-1 mt-1 text-xs text-sky-400">
              <TrendingDown className="h-3 w-3" />
              <span>-3.2% from last month</span>
            </div>
          </CardContent>
        </Card>

        <Card className="relative overflow-hidden border-border/50 bg-gradient-to-br from-card to-card/50">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Active Loans
            </CardTitle>
            <div className="h-8 w-8 rounded-lg bg-primary/10 flex items-center justify-center">
              <FileText className="h-4 w-4 text-primary" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {statsLoading ? "—" : stats?.TotalLoans}
            </div>
            <div className="mt-2">
              <Progress value={65} max={100} variant="success" className="h-1.5" />
            </div>
          </CardContent>
        </Card>

        <Card className="relative overflow-hidden border-border/50 bg-gradient-to-br from-card to-card/50">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Active Applications
            </CardTitle>
            <div className="h-8 w-8 rounded-lg bg-amber-500/10 flex items-center justify-center">
              <Users className="h-4 w-4 text-amber-500" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {statsLoading ? "—" : stats?.ActiveApplications}
            </div>
            <div className="mt-1 text-xs text-muted-foreground">
              3 awaiting review
            </div>
          </CardContent>
        </Card>

        <Card className="relative overflow-hidden border-border/50 bg-gradient-to-br from-card to-card/50">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Approval Rate
            </CardTitle>
            <div className="h-8 w-8 rounded-lg bg-violet-500/10 flex items-center justify-center">
              <TrendingUp className="h-4 w-4 text-violet-500" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {statsLoading ? "—" : `${stats?.ApprovalRate}%`}
            </div>
            <div className="mt-2">
              <Progress value={stats?.ApprovalRate || 0} max={100} variant="success" className="h-1.5" />
            </div>
          </CardContent>
        </Card>

        <Card className="relative overflow-hidden border-border/50 bg-gradient-to-br from-card to-card/50">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Avg. Loan Amount
            </CardTitle>
            <div className="h-8 w-8 rounded-lg bg-rose-500/10 flex items-center justify-center">
              <Banknote className="h-4 w-4 text-rose-500" />
            </div>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {statsLoading ? "—" : formatZAR(stats?.AvgLoanAmount || 0)}
            </div>
            <div className="mt-1 text-xs text-muted-foreground">
              Across all products
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Charts */}
      <div className="grid gap-4 lg:grid-cols-2">
        <Card className="border-border/50">
          <CardHeader>
            <CardTitle>Monthly Disbursements</CardTitle>
            <CardDescription>Loan disbursement volume over the last 6 months</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={280}>
              <BarChart data={monthlyData}>
                <CartesianGrid strokeDasharray="3 3" stroke="hsl(217, 33%, 15%)" />
                <XAxis dataKey="month" stroke="hsl(215, 20%, 55%)" fontSize={12} tickLine={false} axisLine={false} />
                <YAxis stroke="hsl(215, 20%, 55%)" fontSize={12} tickLine={false} axisLine={false} tickFormatter={(v) => `R${v / 1000}k`} />
                <Tooltip
                  contentStyle={{
                    backgroundColor: "hsl(222, 47%, 7%)",
                    border: "1px solid hsl(217, 33%, 15%)",
                    borderRadius: "8px",
                    color: "hsl(210, 40%, 98%)",
                  }}
                  formatter={(value: unknown) => [formatZAR(value as number), "Disbursed"]}
                />
                <Bar dataKey="disbursed" fill="hsl(160, 84%, 39%)" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card className="border-border/50">
          <CardHeader>
            <CardTitle>Loan Status Distribution</CardTitle>
            <CardDescription>Current breakdown by status</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={280}>
              <PieChart>
                <Pie
                  data={statusData}
                  cx="50%"
                  cy="50%"
                  innerRadius={60}
                  outerRadius={100}
                  paddingAngle={4}
                  dataKey="value"
                >
                  {statusData.map((_, index) => (
                    <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip
                  contentStyle={{
                    backgroundColor: "hsl(222, 47%, 7%)",
                    border: "1px solid hsl(217, 33%, 15%)",
                    borderRadius: "8px",
                    color: "hsl(210, 40%, 98%)",
                  }}
                />
              </PieChart>
            </ResponsiveContainer>
            <div className="flex justify-center gap-4 mt-2">
              {statusData.map((entry, index) => (
                <div key={entry.name} className="flex items-center gap-1.5">
                  <div className="h-2.5 w-2.5 rounded-full" style={{ backgroundColor: COLORS[index] }} />
                  <span className="text-xs text-muted-foreground">{entry.name}</span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Recent Applications */}
      <Card className="border-border/50">
        <CardHeader>
          <CardTitle>Recent Applications</CardTitle>
          <CardDescription>Latest loan applications requiring attention</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            {applications.slice(0, 4).map((app) => (
              <div
                key={app.Id}
                className="flex items-center justify-between rounded-lg border border-border/50 p-4 transition-colors hover:bg-muted/30"
              >
                <div className="flex items-center gap-4">
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/10 text-primary font-semibold text-sm">
                    {app.ApplicantName?.charAt(0)}
                  </div>
                  <div>
                    <p className="text-sm font-medium">{app.ApplicantName}</p>
                    <p className="text-xs text-muted-foreground">{app.Purpose}</p>
                  </div>
                </div>
                <div className="flex items-center gap-4">
                  <div className="text-right">
                    <p className="text-sm font-medium">{formatZAR(app.RequestedAmount)}</p>
                    <p className="text-xs text-muted-foreground">{app.RequestedTermMonths} months</p>
                  </div>
                  <Badge
                    variant={
                      app.Status === "Approved"
                        ? "success"
                        : app.Status === "Rejected"
                        ? "destructive"
                        : app.Status === "UnderReview"
                        ? "warning"
                        : "secondary"
                    }
                  >
                    {app.Status}
                  </Badge>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
