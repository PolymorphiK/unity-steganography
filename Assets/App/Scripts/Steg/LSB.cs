using UnityEngine;
using System.Collections.Generic;

public static class LSB {
	public static LSBSample Sample(Color32[] pixels) {
		return new LSBSample {
			Pixels = pixels.Length,
			Bits = pixels.Length * 3,
			MegaBits = pixels.Length * 3 / 1E6,
			Bytes = pixels.Length * 3 / 8,
			KiloBytes = pixels.Length * 3 / 8 / 1024,
			MegaBytes = pixels.Length * 3 / 8 / 1024 / 1024
		};
	}

	public static void Encode(Color32[] pixels, byte[] data, System.Action<Code> onComplete, System.Action<float> onProgress = null) {
		// Conduct calculations
		// on an other thread...
		uThread.RunTaskAsync(() => {
			byte[] color = new byte[3];

			int total_bits = pixels.Length * 3;
			int bits_for_message = data.Length * 8;

			if((bits_for_message + 8) >= total_bits) {
				onComplete?.Invoke(Code.Error);
				return;
			}

			// copy RGB values
			// for ease of access
			color[0] = pixels[0].r;
			color[1] = pixels[0].g;
			color[2] = pixels[0].b;

			int color_index = 0;
			int pixel_index = 0;
			int bit_index = -1;
			int byte_index = -1;
			byte bite = 0;
			byte padding_value = 0;

			// Encoding data into colors...
			for(int i = 0; i < bits_for_message; ++i) {
				onProgress?.Invoke((float)(i + 1) / (bits_for_message + 8));

				bit_index++;

				if(bit_index % 8 == 0) {
					bit_index = 0;

					bite = data[++byte_index];
				}

				LSB.Set(bite, ref color[color_index]);

				bite = (byte)(bite >> 1);

				color_index = (color_index + 1) % 3;

				if(color_index == 0) {
					pixels[pixel_index].r = color[0];
					pixels[pixel_index].g = color[1];
					pixels[pixel_index].b = color[2];

					pixel_index++;

					color[0] = pixels[pixel_index].r;
					color[1] = pixels[pixel_index].g;
					color[2] = pixels[pixel_index].b;
				}
			}

			// padding...
			for(int i = 0; i < 8; ++i) {
				onProgress?.Invoke((float)(i + 1 + bits_for_message) / (bits_for_message + 8));
				// we are now padding...
				LSB.Set(padding_value, ref color[color_index]);

				color_index = (color_index + 1) % 3;

				if(color_index == 0) {
					pixels[pixel_index].r = color[0];
					pixels[pixel_index].g = color[1];
					pixels[pixel_index].b = color[2];

					pixel_index++;

					color[0] = pixels[pixel_index].r;
					color[1] = pixels[pixel_index].g;
					color[2] = pixels[pixel_index].b;
				}
			}

			uThread.RunOnUnityThread(() => {
				onComplete?.Invoke(Code.Success);
			});
		});
	}

	static void Set(byte source, ref byte target) {
		//Debug.Log("Target: " + target);
		// 1111 1111 TARGET Example
		// 0000 0100 SOURCE Example

		unchecked {
			// 0000 0001 LSB
			byte lsb = (byte)(1 << 0);

			// 1111 1111 TARGET
			// 1111 1110 ~LSB
			// 1111 1110 TARGET & ~LSB

			target = (byte)(target & ~lsb); // unset the LSB

			// 0000 0100 SOURCE
			// 0000 0001 LSB
			// 0000 0000 SOURCE & LSB

			// 1111 1110 TARGET
			// 0000 0000 SOURCE & LSB
			// 1111 1110 TARGET | (SOURCE & LSB)
			target = (byte)(target | (source & lsb)); // set the target's LSB to source's LSB
		};

		//Debug.Log("Target After: " + target);
	}

	public static void Decode(Color32[] colors, System.Action<Code, byte[]> onComplete) {
		uThread.RunTaskAsync(
			() => {
				List<byte> data = new List<byte>();
				byte[] color = new byte[3];

				int bit_index = 0;
				byte result = 0;
				byte lsb = 1 << 0;

				for(int i = 0; i < colors.Length; ++i) {
					color[0] = colors[i].r;
					color[1] = colors[i].g;
					color[2] = colors[i].b;

					for(int j = 0; j < 3; ++j) {
						unchecked {
							result = (byte)((result << 1) | color[j] & lsb);
						}

						bit_index++;

						if(bit_index == 8) {
							bit_index = 0;

							// this is a poor way of checking if we are done...but lets do it?
							if(result == 0) {
								uThread.RunOnUnityThread(() => {
									onComplete?.Invoke(Code.Success, data.ToArray());
								});

								return;
							} else {
								result = LSB.Reverse(result);

								data.Add(result);
							}
						}
					}
				}
			});
	}

	static byte Reverse(byte source) {
		byte result = 0;
		byte lsb = 1 << 0;

		unchecked {
			for(int i = 0; i < 8; i++) {
				result = (byte)( result << 1 | source & lsb);

				source = (byte) (source >> 1);
			}
		}

		return result;
	}

	public struct LSBSample {
		public int Pixels;
		public long Bits;
		public double MegaBits;
		public long Bytes;
		public long KiloBytes;
		public long MegaBytes;
	}

	public enum Code {
		Success = 0,
		Error
	}
}