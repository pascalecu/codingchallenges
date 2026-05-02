// swift-tools-version: 6.3
import PackageDescription

let package = Package(
    name: "wc",
    platforms: [
        .macOS(.v13)
    ],
    products: [
        .executable(name: "wc", targets: ["WC"])
    ],
    targets: [
        .target(name: "WCCore"),
        .executableTarget(name: "WC", dependencies: ["WCCore"]),
        .testTarget(
            name: "WCCoreTests",
            dependencies: ["WCCore"],
            resources: [
                .process("Fixtures")
            ]
        ),
    ],
    swiftLanguageModes: [.v6]
)
