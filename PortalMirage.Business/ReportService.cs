using PortalMirage.Business.Abstractions;
using PortalMirage.Core.Dtos;
using PortalMirage.Data.Abstractions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortalMirage.Business
{
    public class ReportService : IReportService
    {
        private readonly IMachineBreakdownRepository _machineBreakdownRepository;
        private readonly IHandoverRepository _handoverRepository;
        private readonly IKitValidationRepository _kitValidationRepository;
        private readonly IRepeatSampleLogRepository _repeatSampleLogRepository;
        private readonly IDailyTaskLogRepository _dailyTaskLogRepository;
        private readonly IMediaSterilityCheckRepository _mediaSterilityCheckRepository;
        private readonly ISampleStorageRepository _sampleStorageRepository;
        private readonly ICalibrationLogRepository _calibrationLogRepository;

        public ReportService(
            IMachineBreakdownRepository machineBreakdownRepository,
            IHandoverRepository handoverRepository,
            IKitValidationRepository kitValidationRepository,
            IRepeatSampleLogRepository repeatSampleLogRepository,
            IDailyTaskLogRepository dailyTaskLogRepository,
            IMediaSterilityCheckRepository mediaSterilityCheckRepository,
            ISampleStorageRepository sampleStorageRepository,
            ICalibrationLogRepository calibrationLogRepository)
        {
            _machineBreakdownRepository = machineBreakdownRepository;
            _handoverRepository = handoverRepository;
            _kitValidationRepository = kitValidationRepository;
            _repeatSampleLogRepository = repeatSampleLogRepository;
            _dailyTaskLogRepository = dailyTaskLogRepository;
            _mediaSterilityCheckRepository = mediaSterilityCheckRepository;
            _sampleStorageRepository = sampleStorageRepository;
            _calibrationLogRepository = calibrationLogRepository;

            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ==========================================
        // DATA RETRIEVAL METHODS
        // ==========================================

        public async Task<DailyTaskComplianceReportDto> GetDailyTaskComplianceReportAsync(DateTime startDate, DateTime endDate, int? shiftId, string? status)
        {
            var items = (await _dailyTaskLogRepository.GetComplianceReportDataAsync(startDate, endDate, shiftId, status)).ToList();

            // FIX: Check for both "Completed" and "Complete" (case-insensitive)
            var completedCount = items.Count(i =>
                !string.IsNullOrWhiteSpace(i.Status) &&
                (i.Status.Trim().Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                 i.Status.Trim().Equals("Complete", StringComparison.OrdinalIgnoreCase)));

            return new DailyTaskComplianceReportDto(items, items.Count, completedCount);
        }

        public async Task<IEnumerable<HandoverReportDto>> GetHandoverReportAsync(DateTime startDate, DateTime endDate, string? shift, string? priority, string? status)
        {
            return await _handoverRepository.GetReportDataAsync(startDate, endDate, shift, priority, status);
        }

        public async Task<IEnumerable<KitValidationReportDto>> GetKitValidationReportAsync(DateTime startDate, DateTime endDate, string? kitName, string? status)
        {
            return await _kitValidationRepository.GetReportDataAsync(startDate, endDate, kitName, status);
        }

        public async Task<IEnumerable<MachineBreakdownReportDto>> GetMachineBreakdownReportAsync(DateTime startDate, DateTime endDate, string? machineName, string? status)
        {
            return await _machineBreakdownRepository.GetReportDataAsync(startDate, endDate, machineName, status);
        }

        public async Task<IEnumerable<RepeatSampleReportDto>> GetRepeatSampleReportAsync(DateTime startDate, DateTime endDate, string? reason, string? department)
        {
            return await _repeatSampleLogRepository.GetReportDataAsync(startDate, endDate, reason, department);
        }

        public async Task<IEnumerable<MediaSterilityReportDto>> GetMediaSterilityReportAsync(DateTime startDate, DateTime endDate, string? mediaName, string? status)
        {
            return await _mediaSterilityCheckRepository.GetReportDataAsync(startDate, endDate, mediaName, status);
        }

        public async Task<IEnumerable<SampleStorageReportDto>> GetSampleStorageReportAsync(DateTime startDate, DateTime endDate, string? testName, string? status)
        {
            return await _sampleStorageRepository.GetReportDataAsync(startDate, endDate, testName, status);
        }

        public async Task<IEnumerable<CalibrationReportDto>> GetCalibrationReportAsync(DateTime startDate, DateTime endDate, string? testName, string? qcResult)
        {
            return await _calibrationLogRepository.GetReportDataAsync(startDate, endDate, testName, qcResult);
        }

        // ==========================================
        // PDF GENERATION METHODS
        // ==========================================

        public byte[] GenerateMachineBreakdownReport(List<MachineBreakdownReportDto> data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("Machine Breakdown Report").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingTop(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Machine Name").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Breakdown Reason").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Reported By").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Downtime").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Resolution Notes").SemiBold();
                        });

                        foreach (var item in data)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.MachineName ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.BreakdownReason ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.ReportedByUserName ?? "Unknown");

                            string duration = "Pending";
                            if (item.DowntimeMinutes.HasValue)
                            {
                                TimeSpan ts = TimeSpan.FromMinutes(item.DowntimeMinutes.Value);
                                duration = ts.TotalDays >= 1 ? $"{ts.Days}d {ts.Hours}h" : $"{ts.Hours}h {ts.Minutes}m";
                            }
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(duration);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.ResolutionNotes ?? "-");
                        }
                    });
                });
            }).GeneratePdf();
        }

        public byte[] GenerateHandoverReport(List<HandoverReportDto> data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("Shift Handover Report").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingTop(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn(3);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Given By").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Shift").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Handover Notes").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Received By").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Received At").SemiBold();
                        });

                        foreach (var item in data)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.GivenByUserName ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.Shift ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.HandoverNotes ?? "-");

                            if (!string.IsNullOrEmpty(item.ReceivedByUserName))
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.ReceivedByUserName);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.ReceivedDateTime?.ToString("g") ?? "-");
                            }
                            else
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text("NOT RECEIVED").FontColor(Colors.Red.Medium);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text("-");
                            }
                        }
                    });
                });
            }).GeneratePdf();
        }

        public byte[] GenerateRepeatSampleReport(List<RepeatSampleReportDto> data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("Repeat Sample Log").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingTop(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn(2);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Patient Name").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Patient ID").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Reason").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Department").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Informed Person").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Logged By").SemiBold();
                        });

                        foreach (var item in data)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.PatientName ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.PatientIdCardNumber ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.ReasonText ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.Department ?? "General");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.InformedPerson ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.LoggedByUserName ?? "-");
                        }
                    });
                });
            }).GeneratePdf();
        }

        // ==========================================
        // UPDATED METHOD: GenerateDailyTaskReport
        // ==========================================
        public byte[] GenerateDailyTaskReport(List<DailyTaskLogDto> logs, DateTime startDate, DateTime endDate)
        {
            // --- 1. Fix Compliance Calculation Logic ---
            int total = logs.Count; // Total includes Pending, Incomplete, and Completed

            // Count ONLY valid "Completed" tasks (Case insensitive)
            int completed = logs.Count(x => !string.IsNullOrWhiteSpace(x.Status) &&
                                           (x.Status.Trim().Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                                            x.Status.Trim().Equals("Complete", StringComparison.OrdinalIgnoreCase)));

            int pending = total - completed;

            // Calculate percentage (0 to 100)
            // We use (double) to ensure decimal division, then multiply by 100.
            double percentage = total > 0 ? ((double)completed / total) * 100 : 0;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Daily Task Report").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium).AlignCenter();
                        col.Item().Text($"Period: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}").FontSize(10).AlignCenter();
                    });

                    page.Content().PaddingTop(1, Unit.Centimetre).Column(col =>
                    {
                        // --- 2. Performance Summary Table ---
                        col.Item().PaddingBottom(20).Width(300).Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.RelativeColumn(); c.RelativeColumn(); });

                            table.Header(h => h.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text("Performance Summary").SemiBold());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Total Tasks:");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(total.ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Completed:");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(completed.ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Pending/Missed:");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(pending.ToString());

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Completion Rate:").SemiBold();

                            // Logic: If percentage is 22.2, format as "22.2%"
                            var percentColor = percentage < 100 ? Colors.Red.Medium : Colors.Green.Medium;
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{percentage:F1}%").SemiBold().FontColor(percentColor);
                        });

                        // --- 3. Main Detail Table (7 Columns) ---
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1.5f); // 1. Date
                                c.RelativeColumn(3);    // 2. Task Name
                                c.RelativeColumn(1);    // 3. Shift
                                c.RelativeColumn(1.5f); // 4. Status
                                c.RelativeColumn(1.5f); // 5. Completed On
                                c.RelativeColumn(2);    // 6. Completed By
                                c.RelativeColumn(3);    // 7. Comments
                            });

                            table.Header(h =>
                            {
                                void HeaderCell(string text) => h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text(text).SemiBold();
                                HeaderCell("Date");
                                HeaderCell("Task Name");
                                HeaderCell("Shift");
                                HeaderCell("Status");
                                HeaderCell("Completed On");
                                HeaderCell("Completed By");
                                HeaderCell("Comments");
                            });

                            foreach (var item in logs)
                            {
                                IContainer DataCell() => table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);

                                // 1. Date
                                DataCell().Text(item.TaskDate.ToString("dd/MMM/yy HH:mm"));

                                // 2. Task Name
                                DataCell().Text(item.TaskName ?? "-");

                                // 3. Shift 
                                // WARNING: Your screenshot showed "Pending" here. If that happens, your DB is mapping Status to Shift.
                                DataCell().Text(item.Shift ?? "-");

                                // 4. Status
                                DataCell().Text(item.Status ?? "Pending");

                                // 5. Completed On
                                var compDate = item.CompletedDateTime.HasValue ? item.CompletedDateTime.Value.ToString("dd/MMM/yy HH:mm") : "-";
                                DataCell().Text(compDate);

                                // 6. Completed By
                                // WARNING: Your screenshot showed a Date here. If that happens, your DB is mapping Date to UserName.
                                DataCell().Text(item.CompletedByUserName ?? "-");

                                // 7. Comments
                                DataCell().Text(item.Comments ?? "");
                            }
                        });
                    });
                });
            }).GeneratePdf();
        }

        public byte[] GenerateKitValidationReport(List<KitValidationReportDto> data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("Kit Validation Report").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingTop(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Kit Name").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Kit Lot Number").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Expiry Date").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Status").SemiBold();
                            header.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Validated By").SemiBold();
                        });

                        foreach (var item in data)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.KitName ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.KitLotNumber ?? "-");

                            // SAFE DATE
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.KitExpiryDate.ToString("yyyy-MM-dd"));

                            // FIX: Case-insensitive status check
                            var isValid = item.ValidationStatus?.Trim().Equals("Valid", StringComparison.OrdinalIgnoreCase) == true;
                            var statusColor = isValid ? Colors.Green.Medium : Colors.Red.Medium;

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.ValidationStatus ?? "-").FontColor(statusColor);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.ValidatedByUserName ?? "-");
                        }
                    });
                });
            }).GeneratePdf();
        }

        public byte[] GenerateMediaSterilityReport(List<MediaSterilityReportDto> data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("Media Sterility Report").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingTop(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(2); });

                        table.Header(h =>
                        {
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Media").SemiBold();
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Lot").SemiBold();
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Results").SemiBold();
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Status").SemiBold();
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("User").SemiBold();
                        });

                        foreach (var item in data)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.MediaName ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.MediaLotNumber ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text($"{item.Result37C ?? "-"}/{item.Result25C ?? "-"}");

                            var isSterile = item.OverallStatus?.Trim().Equals("Sterile", StringComparison.OrdinalIgnoreCase) == true;
                            var color = isSterile ? Colors.Green.Medium : Colors.Red.Medium;

                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.OverallStatus ?? "-").FontColor(color);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.PerformedByUserName ?? "-");
                        }
                    });
                });
            }).GeneratePdf();
        }

        public byte[] GenerateSampleStorageReport(List<SampleStorageReportDto> data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("Sample Storage Report").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingTop(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2); });

                        table.Header(h =>
                        {
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Sample ID").SemiBold();
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Test").SemiBold();
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Date").SemiBold();
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Status").SemiBold();
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("User").SemiBold();
                        });

                        foreach (var item in data)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.PatientSampleID ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.TestName ?? "-");

                            // Safe date check
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.StorageDateTime.ToString("yyyy-MM-dd"));

                            var color = item.IsTestDone ? Colors.Green.Medium : Colors.Blue.Medium;
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.IsTestDone ? "Completed" : "Stored").FontColor(color);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.StoredByUserName ?? "-");
                        }
                    });
                });
            }).GeneratePdf();
        }

        public byte[] GenerateCalibrationReport(List<CalibrationReportDto> data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.Header().Text("Calibration Report").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingTop(1, Unit.Centimetre).Table(table =>
                    {
                        table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(1); c.RelativeColumn(2); c.RelativeColumn(2); });

                        table.Header(h =>
                        {
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Equipment").SemiBold();
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Date").SemiBold();
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("QC").SemiBold();
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("Reason").SemiBold();
                            h.Cell().BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5).Text("User").SemiBold();
                        });

                        foreach (var item in data)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.TestName ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.CalibrationDateTime.ToString("yyyy-MM-dd"));

                            var isPass = item.QcResult?.Trim().Equals("Pass", StringComparison.OrdinalIgnoreCase) == true
                                        || item.QcResult?.Trim().Equals("In Range", StringComparison.OrdinalIgnoreCase) == true;

                            var color = isPass ? Colors.Green.Medium : Colors.Red.Medium;
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.QcResult ?? "-").FontColor(color);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.Reason ?? "-");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5).Text(item.PerformedByUserName ?? "-");
                        }
                    });
                });
            }).GeneratePdf();
        }
    }
}