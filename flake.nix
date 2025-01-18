{
  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixpkgs-unstable";
  };

  outputs = {
    self,
    nixpkgs,
  }: let
    pkgs = nixpkgs.legacyPackages."x86_64-linux";
    lib = nixpkgs.lib;
  in {
    devShells."x86_64-linux".default = pkgs.mkShell {
      packages = with pkgs; [
        dotnet-sdk_9
        dotnet-runtime_9
        dotnet-aspnetcore_9
        wmctrl

        omnisharp-roslyn
        dotnetPackages.Nuget
        # avalonia
      ];

      LD_LIBRARY_PATH = with pkgs;
        lib.makeLibraryPath [
          stdenv.cc.cc
          fontconfig
          xorg.libX11
          xorg.libICE
          xorg.libSM
          zlib
          openssl
        ];
      NIX_LD = "${pkgs.stdenv.cc}/bin/ld";

      DOTNET_ROOT = "${pkgs.dotnet-sdk_9}";
    };
  };
}
