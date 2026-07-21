using DyingMessage;
using DyingMessage.Core;
using DyingMessage.Storage;

var builder = Host.CreateApplicationBuilder(args);

// 1. Windows 서비스 연동 (서비스 등록 시 필요, 없을 시 일반 콘솔/Worker로 동작)
builder.Services.AddWindowsService();

// 2. 하드웨어 수집기 등록 (싱글톤으로 인스턴스 유지 -> GC 부하 최적화)
builder.Services.AddSingleton<IHardwareCollector, HardwareCollector>();

// 3. 강종 대비 WAL 저장소 등록 (SQLite)
builder.Services.AddSingleton<ILocalRepository, SQLiteRepository>();

// 4. 1초 주기 수집 백그라운드 Worker 등록
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// 백그라운드 서비스 실행
host.Run();