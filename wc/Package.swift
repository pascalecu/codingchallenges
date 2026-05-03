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
    dependencies: [
        .package(url: "https://github.com/apple/swift-argument-parser.git", from: "1.7.1")
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
