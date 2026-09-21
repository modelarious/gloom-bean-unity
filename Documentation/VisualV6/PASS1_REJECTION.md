# First integrated capture rejected

The native capture command succeeded, but its images are visually worse: replacement platforms and HUD icons are invisible, and the old background fallback remains. This is NOT an art improvement or a passing comparative review. Preserved images: Evidence/Pass1-Rejected.

Direct import evidence showed PNGs serialized with textureShape=2 (cubemap), so typed Texture2D loads did not resolve. The new importer explicitly requests TextureImporterShape.Texture2D and advances its import version. Resource type/count and pixel filter contracts now fail the visual fixture and native presentation tests. Render proxies also keep the original visible when replacement art is missing. Original failure images remain; no score is upgraded on a successful capture command alone.

The large stage caption also used uppercase; case-insensitive exact matching now removes the duplicate decorative title without removing functional signs.
