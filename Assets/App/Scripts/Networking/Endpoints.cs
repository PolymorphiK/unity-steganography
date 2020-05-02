public static class Endpoints {
	public static readonly string Endpoint = "http://athena.ecs.csus.edu/~pachecok/153";

	public static class Get {
		public static readonly string Images = "/images.php";
	}

	public static class Post {
		public static readonly string Images = "/images.php";
	}

	static string Bind(string endpoint, string action) {
		return string.Format("{0}{1}", endpoint, action);
	}
}