import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

const String apiUrl = 'https://localhost:7028/'; // backend-api

/*
http.post(
  Uri.parse('${apiUrl}auth/login'),
  body: {
    'Username': username,
    'Password': password,
  },
)

http.post(
  Uri.parse('${apiUrl}query'),
  body: {
    'Query': query,
  },
)
*/

Future<String?> getTokenFromLocalStorage() async {
  final prefs = await SharedPreferences.getInstance();
  final token = prefs.getString('access_token');
  return token;
}

Future<Map<String, dynamic>?> loginUser(String username, String password) async {
  final response = await http.post(
    Uri.parse('${apiUrl}auth/login'),
    headers: <String, String>{
      'Content-Type': 'application/json',
    },
    body: jsonEncode(<String, String>{
      'Username': username,
      'Password': password,
    }),
  );
  print('Body: ${jsonEncode(response)}');

  if (response.statusCode == 200) {
    // Extract token from response
    final jsonResponse = jsonDecode(response.body);
    final token = jsonResponse['Token'];

    // Save token to local storage
    final prefs = await SharedPreferences.getInstance();
    prefs.setString('access_token', token);

    Map<String, dynamic> body = {
      'id': jsonResponse['Id'],
      'username': jsonResponse['Username']
    };

    // Output the results
    print('Token: $token');
    print('Body: ${jsonEncode(body)}');

    return body;

  } else return null;
}

Future<Map<String, dynamic>?> getUser(String userId) async {
  final token = await getTokenFromLocalStorage();
  final response = await http.get(
    Uri.parse('${apiUrl}user/$userId'),
    headers: <String, String>{
      'Authorization': 'Bearer $token',
    },
  );

  if (response.statusCode == 200) {
    final jsonResponse = jsonDecode(response.body);

    Map<String, dynamic> body = {
      'user': jsonResponse['Data']
    };
    return body;
  } else return null;
}

Future<Map<String, dynamic>?> postQuery(String query) async {
  final token = await getTokenFromLocalStorage();
  final response = await http.post(
    Uri.parse('${apiUrl}query'),
    headers: <String, String>{
      'Content-Type': 'application/json',
      'Authorization': 'Bearer $token',
    },
    body: jsonEncode(<String, String>{
      'Query': query,
    }),
  );

  if (response.statusCode == 200) {
    // Handle response from ChatGPT API
    final jsonResponse = jsonDecode(response.body);

    Map<String, dynamic> body = {
      'response': jsonResponse['Data']
    };
    return body;
  } else return null;
}

Future<Map<String, dynamic>?> getUserQueries(String userId) async {
  final token = await getTokenFromLocalStorage();
  final response = await http.get(
    Uri.parse('${apiUrl}query/$userId'),
    headers: <String, String>{
      'Authorization': 'Bearer $token',
    },
  );

  if (response.statusCode == 200) {
    final jsonResponse = jsonDecode(response.body);

    Map<String, dynamic> body = {
      'queries': jsonResponse['Data']
    };
    return body;
  } else return null;
}
